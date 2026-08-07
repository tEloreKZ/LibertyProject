#pragma once

#include <memory>

struct IDXGISwapChain;

namespace liberty {

class Browser;

class IGraphicsBackend {
 public:
  virtual ~IGraphicsBackend() = default;
  virtual bool Initialize(IDXGISwapChain* swap_chain) = 0;
  virtual void Resize(int width, int height) = 0;
  virtual void UpdateBrowserTexture(const std::shared_ptr<Browser>& browser) = 0;
  virtual void DrawBrowser(const std::shared_ptr<Browser>& browser) = 0;
  virtual void Shutdown() = 0;
};

}  // namespace liberty

