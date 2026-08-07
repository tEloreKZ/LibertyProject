#include "Cef/CefSchemeHandler.h"

#include <algorithm>
#include <unordered_map>

#include "Logging/NativeLogger.h"
#include "include/cef_parser.h"
#include "include/cef_stream.h"
#include "include/wrapper/cef_stream_resource_handler.h"

namespace liberty {
namespace {

bool IsWithin(const std::filesystem::path& candidate, const std::filesystem::path& root) {
  const auto canonical_candidate = std::filesystem::weakly_canonical(candidate);
  const auto canonical_root = std::filesystem::weakly_canonical(root);
  auto root_iterator = canonical_root.begin();
  auto candidate_iterator = canonical_candidate.begin();
  for (; root_iterator != canonical_root.end(); ++root_iterator, ++candidate_iterator) {
    if (candidate_iterator == canonical_candidate.end() || *candidate_iterator != *root_iterator)
      return false;
  }
  return true;
}

std::filesystem::path Utf8Path(const std::string& value) {
  const auto* first = reinterpret_cast<const char8_t*>(value.data());
  return std::filesystem::path(std::u8string(first, first + value.size()));
}

std::string MimeType(const std::filesystem::path& path) {
  static const std::unordered_map<std::string, std::string> types{
      {".html", "text/html"}, {".htm", "text/html"}, {".js", "text/javascript"},
      {".mjs", "text/javascript"}, {".css", "text/css"}, {".json", "application/json"},
      {".png", "image/png"}, {".jpg", "image/jpeg"}, {".jpeg", "image/jpeg"},
      {".svg", "image/svg+xml"}, {".webp", "image/webp"}, {".woff", "font/woff"},
      {".woff2", "font/woff2"}, {".ttf", "font/ttf"}, {".ico", "image/x-icon"}};
  auto extension = path.extension().string();
  std::ranges::transform(extension, extension.begin(), [](unsigned char value) {
    return static_cast<char>(std::tolower(value));
  });
  const auto iterator = types.find(extension);
  return iterator == types.end() ? "application/octet-stream" : iterator->second;
}

}  // namespace

LibertySchemeHandlerFactory::LibertySchemeHandlerFactory(std::filesystem::path content_root)
    : content_root_(std::filesystem::weakly_canonical(std::move(content_root))) {}

CefRefPtr<CefResourceHandler> LibertySchemeHandlerFactory::Create(
    CefRefPtr<CefBrowser>, CefRefPtr<CefFrame>, const CefString&,
    CefRefPtr<CefRequest> request) {
  CefURLParts parts;
  if (!CefParseURL(request->GetURL(), parts)) return nullptr;
  const std::string host = CefString(&parts.host);
  std::string relative = CefString(&parts.path);
  while (!relative.empty() && (relative.front() == '/' || relative.front() == '\\'))
    relative.erase(relative.begin());
  if (relative.empty()) relative = "index.html";

  std::filesystem::path root;
  if (host == "ui") root = content_root_ / L"ui";
  else if (host == "mods") {
    const auto separator = relative.find('/');
    if (separator == std::string::npos) return nullptr;
    const auto mod_name = relative.substr(0, separator);
    if (mod_name.empty() || mod_name == "." || mod_name == ".." ||
        mod_name.find('\\') != std::string::npos) return nullptr;
    root = content_root_ / L"mods" / Utf8Path(mod_name) / L"ui";
    relative.erase(0, separator + 1);
    if (relative.empty()) relative = "index.html";
  }
  else return nullptr;
  const auto candidate = root / Utf8Path(relative);
  if (!IsWithin(candidate, root) || !std::filesystem::is_regular_file(candidate)) {
    NativeLogger::Instance().Warning("[CEF] Blocked or missing liberty resource: " +
                                     request->GetURL().ToString());
    return nullptr;
  }
  const auto stream = CefStreamReader::CreateForFile(candidate.wstring());
  return stream ? new CefStreamResourceHandler(MimeType(candidate), stream) : nullptr;
}

}  // namespace liberty
