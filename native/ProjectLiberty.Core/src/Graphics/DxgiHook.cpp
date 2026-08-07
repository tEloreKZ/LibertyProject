#include "Graphics/DxgiHook.h"

#include <Windows.h>

#include <d3d11.h>
#include <d3d12.h>
#include <dxgi.h>
#include <MinHook.h>
#include <mutex>
#include <wrl/client.h>

#include "BrowserManager.h"
#include "Graphics/D3D11Backend.h"
#include "InputManager.h"
#include "Logging/NativeLogger.h"

namespace liberty {
namespace {

using PresentFunction = HRESULT(__stdcall*)(IDXGISwapChain*, UINT, UINT);
using ResizeFunction = HRESULT(__stdcall*)(IDXGISwapChain*, UINT, UINT, UINT, DXGI_FORMAT, UINT);
using ExecuteCommandListsFunction = void(__stdcall*)(ID3D12CommandQueue*, UINT,
                                                      ID3D12CommandList* const*);
PresentFunction original_present{};
ResizeFunction original_resize{};
ExecuteCommandListsFunction original_execute_command_lists{};
D3D11Backend backend;
std::mutex command_queue_mutex;
Microsoft::WRL::ComPtr<ID3D12CommandQueue> command_queue;

void __stdcall ExecuteCommandListsHook(ID3D12CommandQueue* queue, const UINT count,
                                       ID3D12CommandList* const* lists) {
  if (queue && queue->GetDesc().Type == D3D12_COMMAND_LIST_TYPE_DIRECT) {
    std::scoped_lock lock(command_queue_mutex);
    if (command_queue.Get() != queue) command_queue = queue;
  }
  original_execute_command_lists(queue, count, lists);
}

Microsoft::WRL::ComPtr<ID3D12CommandQueue> CurrentCommandQueue() {
  std::scoped_lock lock(command_queue_mutex);
  return command_queue;
}

HRESULT __stdcall PresentHook(IDXGISwapChain* swap_chain, const UINT sync_interval,
                              const UINT flags) {
  // Keep the graphics subsystem completely dormant until a browser is visible.
  // Initializing D3D11-on-12 against GTA Enhanced's swap chain during Story Mode
  // startup is unnecessary and can conflict with the game's own D3D12 lifecycle.
  const auto visible_browsers = BrowserManager::Instance().VisibleBrowsers();
  if (visible_browsers.empty())
    return original_present(swap_chain, sync_interval, flags);

  const auto d3d11_ready = backend.Initialize(swap_chain);
  Microsoft::WRL::ComPtr<ID3D12CommandQueue> queue;
  if (!d3d11_ready) queue = CurrentCommandQueue();
  if (d3d11_ready || backend.InitializeOn12(swap_chain, queue.Get())) {
    DXGI_SWAP_CHAIN_DESC description{};
    if (SUCCEEDED(swap_chain->GetDesc(&description)))
      InputManager::Instance().Attach(description.OutputWindow);
    backend.Render(visible_browsers);
  }
  return original_present(swap_chain, sync_interval, flags);
}

HRESULT __stdcall ResizeHook(IDXGISwapChain* swap_chain, const UINT buffer_count,
                             const UINT width, const UINT height, const DXGI_FORMAT format,
                             const UINT flags) {
  backend.Resize(width, height);
  return original_resize(swap_chain, buffer_count, width, height, format, flags);
}

LRESULT CALLBACK DummyWindowProcedure(HWND window, UINT message, WPARAM wparam, LPARAM lparam) {
  return DefWindowProcW(window, message, wparam, lparam);
}

}  // namespace

bool DxgiHook::Initialize() {
  if (initialized_.load()) return true;
  WNDCLASSEXW window_class{sizeof(window_class)};
  window_class.lpfnWndProc = DummyWindowProcedure;
  window_class.hInstance = GetModuleHandleW(nullptr);
  window_class.lpszClassName = L"ProjectLiberty.DxgiProbe";
  RegisterClassExW(&window_class);
  HWND window = CreateWindowExW(0, window_class.lpszClassName, L"", WS_OVERLAPPEDWINDOW,
                                0, 0, 100, 100, nullptr, nullptr, window_class.hInstance, nullptr);
  if (!window) return false;
  DXGI_SWAP_CHAIN_DESC description{};
  description.BufferCount = 1;
  description.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
  description.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
  description.OutputWindow = window;
  description.SampleDesc.Count = 1;
  description.Windowed = TRUE;
  Microsoft::WRL::ComPtr<IDXGISwapChain> swap_chain;
  Microsoft::WRL::ComPtr<ID3D11Device> device;
  Microsoft::WRL::ComPtr<ID3D11DeviceContext> context;
  const auto result = D3D11CreateDeviceAndSwapChain(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr,
      0, nullptr, 0, D3D11_SDK_VERSION, &description, &swap_chain, &device, nullptr, &context);
  if (FAILED(result)) {
    DestroyWindow(window);
    UnregisterClassW(window_class.lpszClassName, window_class.hInstance);
    return false;
  }
  auto table = *reinterpret_cast<void***>(swap_chain.Get());

  Microsoft::WRL::ComPtr<ID3D12Device> device12;
  Microsoft::WRL::ComPtr<ID3D12CommandQueue> probe_queue;
  void* execute_address{};
  if (SUCCEEDED(D3D12CreateDevice(nullptr, D3D_FEATURE_LEVEL_11_0,
                                  IID_PPV_ARGS(&device12)))) {
    D3D12_COMMAND_QUEUE_DESC queue_description{};
    queue_description.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    if (SUCCEEDED(device12->CreateCommandQueue(&queue_description,
                                               IID_PPV_ARGS(&probe_queue)))) {
      auto queue_table = *reinterpret_cast<void***>(probe_queue.Get());
      execute_address = queue_table[10];
    }
  }

  const auto initialize_result = MH_Initialize();
  bool success = (initialize_result == MH_OK || initialize_result == MH_ERROR_ALREADY_INITIALIZED) &&
      MH_CreateHook(table[8], &PresentHook, reinterpret_cast<void**>(&original_present)) == MH_OK &&
      MH_CreateHook(table[13], &ResizeHook, reinterpret_cast<void**>(&original_resize)) == MH_OK &&
      (!execute_address || MH_CreateHook(execute_address, &ExecuteCommandListsHook,
          reinterpret_cast<void**>(&original_execute_command_lists)) == MH_OK) &&
      MH_EnableHook(MH_ALL_HOOKS) == MH_OK;
  swap_chain.Reset(); context.Reset(); device.Reset();
  DestroyWindow(window);
  UnregisterClassW(window_class.lpszClassName, window_class.hInstance);
  initialized_.store(success);
  if (success) NativeLogger::Instance().Info(
      execute_address ? "[CEF] DXGI Present and D3D12 queue hooks installed"
                      : "[CEF] DXGI Present hook installed (D3D11 only)");
  else NativeLogger::Instance().Error("[CEF] Failed to install DXGI Present hook");
  return success;
}

void DxgiHook::Shutdown() {
  if (!initialized_.exchange(false)) return;
  InputManager::Instance().Detach();
  MH_DisableHook(MH_ALL_HOOKS);
  MH_Uninitialize();
  {
    std::scoped_lock lock(command_queue_mutex);
    command_queue.Reset();
  }
  backend.Shutdown();
}

}  // namespace liberty
