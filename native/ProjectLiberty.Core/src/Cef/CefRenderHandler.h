#pragma once

#include <memory>

#include "Browser.h"
#include "include/cef_render_handler.h"

namespace liberty {

class LibertyCefRenderHandler final : public CefRenderHandler {
 public:
  explicit LibertyCefRenderHandler(std::shared_ptr<Browser> browser)
      : browser_(std::move(browser)) {}

  void GetViewRect(CefRefPtr<CefBrowser> browser, CefRect& rect) override;
  void OnPaint(CefRefPtr<CefBrowser> browser, PaintElementType type,
               const RectList& dirty_rects, const void* buffer,
               int width, int height) override;

 private:
  std::shared_ptr<Browser> browser_;
  IMPLEMENT_REFCOUNTING(LibertyCefRenderHandler);
  DISALLOW_COPY_AND_ASSIGN(LibertyCefRenderHandler);
};

}  // namespace liberty

