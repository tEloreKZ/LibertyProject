#pragma once

#include <filesystem>

#include "include/cef_scheme.h"

namespace liberty {

class LibertySchemeHandlerFactory final : public CefSchemeHandlerFactory {
 public:
  explicit LibertySchemeHandlerFactory(std::filesystem::path content_root);

  CefRefPtr<CefResourceHandler> Create(CefRefPtr<CefBrowser> browser,
                                       CefRefPtr<CefFrame> frame,
                                       const CefString& scheme_name,
                                       CefRefPtr<CefRequest> request) override;

 private:
  std::filesystem::path content_root_;
  IMPLEMENT_REFCOUNTING(LibertySchemeHandlerFactory);
  DISALLOW_COPY_AND_ASSIGN(LibertySchemeHandlerFactory);
};

}  // namespace liberty

