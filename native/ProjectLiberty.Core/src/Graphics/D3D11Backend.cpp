#include "Graphics/D3D11Backend.h"

#include <array>

#include <d3dcompiler.h>

#include "Browser.h"
#include "Logging/NativeLogger.h"

namespace liberty {
namespace {

struct Vertex final { float x, y, u, v; };

constexpr char kVertexShader[] = R"(
struct Input { float2 position : POSITION; float2 uv : TEXCOORD0; };
struct Output { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
Output main(Input input) { Output o; o.position=float4(input.position,0,1); o.uv=input.uv; return o; }
)";
constexpr char kPixelShader[] = R"(
Texture2D texture0 : register(t0); SamplerState sampler0 : register(s0);
float4 main(float4 position : SV_POSITION, float2 uv : TEXCOORD0) : SV_TARGET {
  return texture0.Sample(sampler0, uv);
}
)";

}  // namespace

bool D3D11Backend::Initialize(IDXGISwapChain* swap_chain) {
  if (!swap_chain) return false;
  Microsoft::WRL::ComPtr<ID3D11Device> device;
  if (FAILED(swap_chain->GetDevice(IID_PPV_ARGS(&device)))) return false;
  if (device_ && device_.Get() == device.Get() && render_target_) return true;
  Shutdown();
  swap_chain_ = swap_chain;
  device_ = std::move(device);
  device_->GetImmediateContext(&context_);
  if (!CreatePipeline() || !CreateRenderTarget()) {
    NativeLogger::Instance().Error("[CEF] D3D11 backend initialization failed");
    Shutdown();
    return false;
  }
  NativeLogger::Instance().Info("[CEF] D3D11 rendering backend initialized");
  return true;
}

bool D3D11Backend::InitializeOn12(IDXGISwapChain* swap_chain,
                                 ID3D12CommandQueue* command_queue) {
  if (!swap_chain || !command_queue) return false;
  Microsoft::WRL::ComPtr<ID3D12Device> device12;
  if (FAILED(swap_chain->GetDevice(IID_PPV_ARGS(&device12)))) return false;
  if (using_on12_ && d3d12_device_.Get() == device12.Get() && render_target_) return true;

  Shutdown();
  swap_chain_ = swap_chain;
  swap_chain_.As(&swap_chain3_);
  d3d12_device_ = std::move(device12);
  IUnknown* queues[]{command_queue};
  const D3D_FEATURE_LEVEL feature_levels[]{D3D_FEATURE_LEVEL_11_0};
  D3D_FEATURE_LEVEL selected_level{};
  if (FAILED(D3D11On12CreateDevice(
          d3d12_device_.Get(), D3D11_CREATE_DEVICE_BGRA_SUPPORT,
          feature_levels, static_cast<UINT>(std::size(feature_levels)),
          queues, static_cast<UINT>(std::size(queues)), 0, &device_, &context_,
          &selected_level)) ||
      FAILED(device_.As(&on12_device_))) {
    NativeLogger::Instance().Error("[CEF] D3D11-on-12 device creation failed");
    Shutdown();
    return false;
  }
  using_on12_ = true;
  if (!CreatePipeline() || !CreateWrappedRenderTargets()) {
    NativeLogger::Instance().Error("[CEF] D3D12 interop backend initialization failed");
    Shutdown();
    return false;
  }
  NativeLogger::Instance().Info("[CEF] D3D12 backend initialized through D3D11-on-12");
  return true;
}

void D3D11Backend::Resize(const int width, const int height) {
  if (using_on12_ && context_) {
    context_->ClearState();
    context_->Flush();
  }
  render_target_.Reset();
  wrapped_render_targets_.clear();
  wrapped_back_buffers_.clear();
  target_width_ = width;
  target_height_ = height;
}

void D3D11Backend::UpdateBrowserTexture(const std::shared_ptr<Browser>& browser) {
  if (!device_ || !context_ || !browser) return;
  browser->WithFrame([&](const BrowserFrame& frame) {
    if (frame.bgra.empty()) return;
    auto& resource = textures_[browser->id()];
    if (resource.sequence == frame.sequence) return;
    bool created = false;
    if (!resource.texture || resource.width != frame.width || resource.height != frame.height) {
      resource = {};
      D3D11_TEXTURE2D_DESC description{};
      description.Width = frame.width;
      description.Height = frame.height;
      description.MipLevels = 1;
      description.ArraySize = 1;
      description.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
      description.SampleDesc.Count = 1;
      description.Usage = D3D11_USAGE_DEFAULT;
      description.BindFlags = D3D11_BIND_SHADER_RESOURCE;
      if (FAILED(device_->CreateTexture2D(&description, nullptr, &resource.texture)) ||
          FAILED(device_->CreateShaderResourceView(resource.texture.Get(), nullptr, &resource.view))) {
        NativeLogger::Instance().Error("[CEF] Failed to create browser D3D11 texture");
        resource = {};
        return;
      }
      resource.width = frame.width;
      resource.height = frame.height;
      created = true;
    }
    if (created || frame.dirty_rects.empty()) {
      context_->UpdateSubresource(resource.texture.Get(), 0, nullptr, frame.bgra.data(),
                                  frame.width * 4, 0);
    } else {
      for (const auto& rect : frame.dirty_rects) {
        D3D11_BOX box{static_cast<UINT>(rect.x), static_cast<UINT>(rect.y), 0,
                      static_cast<UINT>(rect.x + rect.width),
                      static_cast<UINT>(rect.y + rect.height), 1};
        const auto* source = frame.bgra.data() +
            (static_cast<std::size_t>(rect.y) * frame.width + rect.x) * 4;
        context_->UpdateSubresource(resource.texture.Get(), 0, &box, source,
                                    frame.width * 4, 0);
      }
    }
    resource.sequence = frame.sequence;
  });
}

void D3D11Backend::DrawBrowser(const std::shared_ptr<Browser>& browser) {
  if (!browser || target_width_ <= 0 || target_height_ <= 0) return;
  const auto iterator = textures_.find(browser->id());
  if (iterator == textures_.end() || !iterator->second.view) return;
  const auto options = browser->options();
  const float left = static_cast<float>(options.x) / target_width_ * 2.0f - 1.0f;
  const float right = static_cast<float>(options.x + options.width) / target_width_ * 2.0f - 1.0f;
  const float top = 1.0f - static_cast<float>(options.y) / target_height_ * 2.0f;
  const float bottom = 1.0f - static_cast<float>(options.y + options.height) / target_height_ * 2.0f;
  const std::array<Vertex, 6> vertices{{
      {left, top, 0, 0}, {right, top, 1, 0}, {left, bottom, 0, 1},
      {left, bottom, 0, 1}, {right, top, 1, 0}, {right, bottom, 1, 1}}};
  D3D11_MAPPED_SUBRESOURCE mapped{};
  if (FAILED(context_->Map(vertex_buffer_.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped))) return;
  std::memcpy(mapped.pData, vertices.data(), sizeof(vertices));
  context_->Unmap(vertex_buffer_.Get(), 0);
  ID3D11ShaderResourceView* view = iterator->second.view.Get();
  context_->PSSetShaderResources(0, 1, &view);
  context_->Draw(6, 0);
}

void D3D11Backend::Render(const std::vector<std::shared_ptr<Browser>>& browsers) {
  if (!context_ || !swap_chain_) return;
  if (!render_target_ && !CreateRenderTarget()) return;
  DXGI_SWAP_CHAIN_DESC description{};
  if (FAILED(swap_chain_->GetDesc(&description))) return;
  target_width_ = static_cast<int>(description.BufferDesc.Width);
  target_height_ = static_cast<int>(description.BufferDesc.Height);
  if (target_width_ <= 0 || target_height_ <= 0) return;

  ID3D11Resource* acquired_resource{};
  if (using_on12_) {
    if (!swap_chain3_ || wrapped_back_buffers_.empty()) return;
    const auto index = swap_chain3_->GetCurrentBackBufferIndex();
    if (index >= wrapped_back_buffers_.size() || index >= wrapped_render_targets_.size()) return;
    render_target_ = wrapped_render_targets_[index];
    acquired_resource = wrapped_back_buffers_[index].Get();
    on12_device_->AcquireWrappedResources(&acquired_resource, 1);
  }

  Microsoft::WRL::ComPtr<ID3D11RenderTargetView> old_target;
  Microsoft::WRL::ComPtr<ID3D11DepthStencilView> old_depth_view;
  context_->OMGetRenderTargets(1, &old_target, &old_depth_view);
  Microsoft::WRL::ComPtr<ID3D11BlendState> old_blend;
  FLOAT blend_factor[4]{}; UINT sample_mask{};
  context_->OMGetBlendState(&old_blend, blend_factor, &sample_mask);
  Microsoft::WRL::ComPtr<ID3D11DepthStencilState> old_depth;
  UINT stencil_reference{}; context_->OMGetDepthStencilState(&old_depth, &stencil_reference);
  Microsoft::WRL::ComPtr<ID3D11RasterizerState> old_rasterizer;
  context_->RSGetState(&old_rasterizer);
  std::array<D3D11_VIEWPORT, D3D11_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE> old_viewports{};
  UINT old_viewport_count = static_cast<UINT>(old_viewports.size());
  context_->RSGetViewports(&old_viewport_count, old_viewports.data());
  Microsoft::WRL::ComPtr<ID3D11InputLayout> old_input_layout;
  context_->IAGetInputLayout(&old_input_layout);
  Microsoft::WRL::ComPtr<ID3D11Buffer> old_vertex_buffer;
  UINT old_stride{}, old_offset{};
  context_->IAGetVertexBuffers(0, 1, &old_vertex_buffer, &old_stride, &old_offset);
  D3D11_PRIMITIVE_TOPOLOGY old_topology{};
  context_->IAGetPrimitiveTopology(&old_topology);
  Microsoft::WRL::ComPtr<ID3D11VertexShader> old_vertex_shader;
  context_->VSGetShader(&old_vertex_shader, nullptr, nullptr);
  Microsoft::WRL::ComPtr<ID3D11PixelShader> old_pixel_shader;
  context_->PSGetShader(&old_pixel_shader, nullptr, nullptr);
  Microsoft::WRL::ComPtr<ID3D11SamplerState> old_sampler;
  context_->PSGetSamplers(0, 1, &old_sampler);
  Microsoft::WRL::ComPtr<ID3D11ShaderResourceView> old_resource;
  context_->PSGetShaderResources(0, 1, &old_resource);

  ID3D11RenderTargetView* target = render_target_.Get();
  context_->OMSetRenderTargets(1, &target, nullptr);
  const float factor[4]{0, 0, 0, 0};
  context_->OMSetBlendState(blend_state_.Get(), factor, 0xffffffff);
  context_->OMSetDepthStencilState(depth_state_.Get(), 0);
  context_->RSSetState(rasterizer_state_.Get());
  D3D11_VIEWPORT viewport{0, 0, static_cast<float>(target_width_),
                          static_cast<float>(target_height_), 0, 1};
  context_->RSSetViewports(1, &viewport);
  const UINT stride = sizeof(Vertex), offset = 0;
  ID3D11Buffer* buffer = vertex_buffer_.Get();
  context_->IASetInputLayout(input_layout_.Get());
  context_->IASetVertexBuffers(0, 1, &buffer, &stride, &offset);
  context_->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
  context_->VSSetShader(vertex_shader_.Get(), nullptr, 0);
  context_->PSSetShader(pixel_shader_.Get(), nullptr, 0);
  ID3D11SamplerState* sampler = sampler_state_.Get();
  context_->PSSetSamplers(0, 1, &sampler);
  for (const auto& browser : browsers) {
    UpdateBrowserTexture(browser);
    DrawBrowser(browser);
  }
  ID3D11ShaderResourceView* empty = nullptr;
  context_->PSSetShaderResources(0, 1, &empty);
  ID3D11RenderTargetView* old_target_pointer = old_target.Get();
  context_->OMSetRenderTargets(1, &old_target_pointer, old_depth_view.Get());
  context_->OMSetBlendState(old_blend.Get(), blend_factor, sample_mask);
  context_->OMSetDepthStencilState(old_depth.Get(), stencil_reference);
  context_->RSSetState(old_rasterizer.Get());
  if (old_viewport_count > 0) context_->RSSetViewports(old_viewport_count, old_viewports.data());
  context_->IASetInputLayout(old_input_layout.Get());
  ID3D11Buffer* old_buffer_pointer = old_vertex_buffer.Get();
  context_->IASetVertexBuffers(0, 1, &old_buffer_pointer, &old_stride, &old_offset);
  context_->IASetPrimitiveTopology(old_topology);
  context_->VSSetShader(old_vertex_shader.Get(), nullptr, 0);
  context_->PSSetShader(old_pixel_shader.Get(), nullptr, 0);
  ID3D11SamplerState* old_sampler_pointer = old_sampler.Get();
  context_->PSSetSamplers(0, 1, &old_sampler_pointer);
  ID3D11ShaderResourceView* old_resource_pointer = old_resource.Get();
  context_->PSSetShaderResources(0, 1, &old_resource_pointer);
  if (using_on12_ && acquired_resource) {
    on12_device_->ReleaseWrappedResources(&acquired_resource, 1);
    context_->Flush();
  }
}

void D3D11Backend::Shutdown() {
  textures_.clear();
  sampler_state_.Reset(); rasterizer_state_.Reset(); depth_state_.Reset();
  blend_state_.Reset(); vertex_buffer_.Reset(); input_layout_.Reset();
  pixel_shader_.Reset(); vertex_shader_.Reset(); render_target_.Reset();
  wrapped_render_targets_.clear(); wrapped_back_buffers_.clear();
  swap_chain3_.Reset(); on12_device_.Reset(); d3d12_device_.Reset();
  context_.Reset(); device_.Reset(); swap_chain_.Reset();
  target_width_ = target_height_ = 0;
  using_on12_ = false;
}

bool D3D11Backend::CreatePipeline() {
  Microsoft::WRL::ComPtr<ID3DBlob> vertex_blob, pixel_blob, errors;
  if (FAILED(D3DCompile(kVertexShader, sizeof(kVertexShader), nullptr, nullptr, nullptr,
                        "main", "vs_4_0", 0, 0, &vertex_blob, &errors)) ||
      FAILED(D3DCompile(kPixelShader, sizeof(kPixelShader), nullptr, nullptr, nullptr,
                        "main", "ps_4_0", 0, 0, &pixel_blob, &errors))) return false;
  if (FAILED(device_->CreateVertexShader(vertex_blob->GetBufferPointer(), vertex_blob->GetBufferSize(),
                                         nullptr, &vertex_shader_)) ||
      FAILED(device_->CreatePixelShader(pixel_blob->GetBufferPointer(), pixel_blob->GetBufferSize(),
                                        nullptr, &pixel_shader_))) return false;
  const D3D11_INPUT_ELEMENT_DESC layout[]{{"POSITION", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0,
      D3D11_INPUT_PER_VERTEX_DATA, 0}, {"TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 8,
      D3D11_INPUT_PER_VERTEX_DATA, 0}};
  if (FAILED(device_->CreateInputLayout(layout, 2, vertex_blob->GetBufferPointer(),
                                        vertex_blob->GetBufferSize(), &input_layout_))) return false;
  D3D11_BUFFER_DESC buffer{}; buffer.ByteWidth = sizeof(Vertex) * 6;
  buffer.Usage = D3D11_USAGE_DYNAMIC; buffer.BindFlags = D3D11_BIND_VERTEX_BUFFER;
  buffer.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
  if (FAILED(device_->CreateBuffer(&buffer, nullptr, &vertex_buffer_))) return false;
  D3D11_BLEND_DESC blend{}; blend.RenderTarget[0].BlendEnable = TRUE;
  blend.RenderTarget[0].SrcBlend = D3D11_BLEND_ONE;
  blend.RenderTarget[0].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
  blend.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
  blend.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
  blend.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_INV_SRC_ALPHA;
  blend.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;
  blend.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;
  if (FAILED(device_->CreateBlendState(&blend, &blend_state_))) return false;
  D3D11_DEPTH_STENCIL_DESC depth{}; depth.DepthEnable = FALSE; depth.StencilEnable = FALSE;
  if (FAILED(device_->CreateDepthStencilState(&depth, &depth_state_))) return false;
  D3D11_RASTERIZER_DESC rasterizer{}; rasterizer.FillMode = D3D11_FILL_SOLID;
  rasterizer.CullMode = D3D11_CULL_NONE; rasterizer.DepthClipEnable = TRUE;
  if (FAILED(device_->CreateRasterizerState(&rasterizer, &rasterizer_state_))) return false;
  D3D11_SAMPLER_DESC sampler{}; sampler.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
  sampler.AddressU = sampler.AddressV = sampler.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
  return SUCCEEDED(device_->CreateSamplerState(&sampler, &sampler_state_));
}

bool D3D11Backend::CreateRenderTarget() {
  if (using_on12_) return CreateWrappedRenderTargets();
  Microsoft::WRL::ComPtr<ID3D11Texture2D> back_buffer;
  return swap_chain_ && SUCCEEDED(swap_chain_->GetBuffer(0, IID_PPV_ARGS(&back_buffer))) &&
         SUCCEEDED(device_->CreateRenderTargetView(back_buffer.Get(), nullptr, &render_target_));
}

bool D3D11Backend::CreateWrappedRenderTargets() {
  if (!using_on12_ || !swap_chain_ || !on12_device_) return false;
  DXGI_SWAP_CHAIN_DESC description{};
  if (FAILED(swap_chain_->GetDesc(&description)) || description.BufferCount == 0) return false;
  wrapped_back_buffers_.clear();
  wrapped_render_targets_.clear();
  wrapped_back_buffers_.reserve(description.BufferCount);
  wrapped_render_targets_.reserve(description.BufferCount);
  D3D11_RESOURCE_FLAGS flags{};
  flags.BindFlags = D3D11_BIND_RENDER_TARGET;
  for (UINT index = 0; index < description.BufferCount; ++index) {
    Microsoft::WRL::ComPtr<ID3D12Resource> resource12;
    Microsoft::WRL::ComPtr<ID3D11Resource> resource11;
    Microsoft::WRL::ComPtr<ID3D11RenderTargetView> target;
    if (FAILED(swap_chain_->GetBuffer(index, IID_PPV_ARGS(&resource12))) ||
        FAILED(on12_device_->CreateWrappedResource(
            resource12.Get(), &flags, D3D12_RESOURCE_STATE_PRESENT,
            D3D12_RESOURCE_STATE_PRESENT, IID_PPV_ARGS(&resource11))) ||
        FAILED(device_->CreateRenderTargetView(resource11.Get(), nullptr, &target))) {
      wrapped_back_buffers_.clear();
      wrapped_render_targets_.clear();
      return false;
    }
    wrapped_back_buffers_.push_back(std::move(resource11));
    wrapped_render_targets_.push_back(std::move(target));
  }
  const auto index = swap_chain3_ ? swap_chain3_->GetCurrentBackBufferIndex() : 0;
  if (index >= wrapped_render_targets_.size()) return false;
  render_target_ = wrapped_render_targets_[index];
  return true;
}

}  // namespace liberty
