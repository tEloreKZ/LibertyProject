#include "Cef/CefRenderHandler.h"

namespace liberty {

void LibertyCefRenderHandler::GetViewRect(CefRefPtr<CefBrowser>, CefRect& rect) {
  const auto options = browser_->options();
  rect = CefRect(0, 0, options.width, options.height);
}

void LibertyCefRenderHandler::OnPaint(CefRefPtr<CefBrowser>, const PaintElementType type,
                                      const RectList& dirty_rects, const void* buffer,
                                      const int width, const int height) {
  if (type != PET_VIEW) return;
  std::vector<BrowserFrameRect> regions;
  regions.reserve(dirty_rects.size());
  for (const auto& rect : dirty_rects)
    regions.push_back({rect.x, rect.y, rect.width, rect.height});
  browser_->UpdateFrame(buffer, width, height, regions);
}

}  // namespace liberty
