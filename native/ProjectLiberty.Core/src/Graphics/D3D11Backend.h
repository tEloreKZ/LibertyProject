#pragma once

#include <cstdint>
#include <memory>
#include <unordered_map>

#include <d3d11.h>
#include <d3d11on12.h>
#include <d3d12.h>
#include <dxgi.h>
#include <dxgi1_4.h>
#include <wrl/client.h>

#include "Graphics/IGraphicsBackend.h"

namespace liberty {

class D3D11Backend final : public IGraphicsBackend {
 public:
  bool Initialize(IDXGISwapChain* swap_chain) override;
  bool InitializeOn12(IDXGISwapChain* swap_chain, ID3D12CommandQueue* command_queue);
  void Resize(int width, int height) override;
  void UpdateBrowserTexture(const std::shared_ptr<Browser>& browser) override;
  void DrawBrowser(const std::shared_ptr<Browser>& browser) override;
  void Shutdown() override;
  void Render(const std::vector<std::shared_ptr<Browser>>& browsers);

 private:
  struct Texture final {
    Microsoft::WRL::ComPtr<ID3D11Texture2D> texture;
    Microsoft::WRL::ComPtr<ID3D11ShaderResourceView> view;
    std::uint64_t sequence{};
    int width{};
    int height{};
  };

  bool CreatePipeline();
  bool CreateRenderTarget();
  bool CreateWrappedRenderTargets();

  Microsoft::WRL::ComPtr<IDXGISwapChain> swap_chain_;
  Microsoft::WRL::ComPtr<ID3D11Device> device_;
  Microsoft::WRL::ComPtr<ID3D11DeviceContext> context_;
  Microsoft::WRL::ComPtr<ID3D12Device> d3d12_device_;
  Microsoft::WRL::ComPtr<ID3D11On12Device> on12_device_;
  Microsoft::WRL::ComPtr<IDXGISwapChain3> swap_chain3_;
  std::vector<Microsoft::WRL::ComPtr<ID3D11Resource>> wrapped_back_buffers_;
  std::vector<Microsoft::WRL::ComPtr<ID3D11RenderTargetView>> wrapped_render_targets_;
  Microsoft::WRL::ComPtr<ID3D11RenderTargetView> render_target_;
  Microsoft::WRL::ComPtr<ID3D11VertexShader> vertex_shader_;
  Microsoft::WRL::ComPtr<ID3D11PixelShader> pixel_shader_;
  Microsoft::WRL::ComPtr<ID3D11InputLayout> input_layout_;
  Microsoft::WRL::ComPtr<ID3D11Buffer> vertex_buffer_;
  Microsoft::WRL::ComPtr<ID3D11BlendState> blend_state_;
  Microsoft::WRL::ComPtr<ID3D11DepthStencilState> depth_state_;
  Microsoft::WRL::ComPtr<ID3D11RasterizerState> rasterizer_state_;
  Microsoft::WRL::ComPtr<ID3D11SamplerState> sampler_state_;
  std::unordered_map<std::uint64_t, Texture> textures_;
  int target_width_{};
  int target_height_{};
  bool using_on12_{};
};

}  // namespace liberty
