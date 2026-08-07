#include "Cef/CefSchemeHandler.h"

#include <algorithm>
#include <cctype>
#include <unordered_map>

#include "Logging/NativeLogger.h"
#include "include/cef_parser.h"
#include "include/cef_stream.h"
#include "include/wrapper/cef_stream_resource_handler.h"

namespace liberty {
namespace {

bool IsWithin(const std::filesystem::path& candidate, const std::filesystem::path& root) {
  std::error_code error;
  const auto canonical_candidate = std::filesystem::weakly_canonical(candidate, error);
  if (error) return false;
  const auto canonical_root = std::filesystem::weakly_canonical(root, error);
  if (error) return false;
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

bool IsSafeModName(const std::string& value) {
  return !value.empty() && value.size() <= 64 &&
      std::ranges::all_of(value, [](const unsigned char character) {
        return std::isalnum(character) || character == '-' || character == '_';
      });
}

CefResponse::HeaderMap SecurityHeaders(const std::filesystem::path& path) {
  CefResponse::HeaderMap headers;
  headers.emplace("Content-Security-Policy",
      "default-src 'self' data: blob:; script-src 'self'; style-src 'self' 'unsafe-inline'; "
      "img-src 'self' data: blob:; font-src 'self' data:; connect-src 'none'; object-src 'none'; "
      "frame-src 'none'; base-uri 'none'; form-action 'none'");
  headers.emplace("X-Content-Type-Options", "nosniff");
  headers.emplace("Referrer-Policy", "no-referrer");
  headers.emplace("Cross-Origin-Resource-Policy", "same-origin");
  const auto extension = path.extension().string();
  headers.emplace("Cache-Control", extension == ".html" || extension == ".htm"
      ? "no-cache" : "public, max-age=31536000, immutable");
  return headers;
}

}  // namespace

LibertySchemeHandlerFactory::LibertySchemeHandlerFactory(std::filesystem::path content_root)
    : content_root_(std::filesystem::weakly_canonical(std::move(content_root))) {}

CefRefPtr<CefResourceHandler> LibertySchemeHandlerFactory::Create(
    CefRefPtr<CefBrowser>, CefRefPtr<CefFrame>, const CefString&,
    CefRefPtr<CefRequest> request) {
  if (!request || request->GetMethod() != "GET") return nullptr;
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
    if (!IsSafeModName(mod_name)) return nullptr;
    root = content_root_ / L"mods" / Utf8Path(mod_name) / L"ui";
    relative.erase(0, separator + 1);
    if (relative.empty()) relative = "index.html";
  }
  else return nullptr;
  const auto candidate = root / Utf8Path(relative);
  std::error_code file_error;
  const auto file_size = std::filesystem::file_size(candidate, file_error);
  const auto regular_file = std::filesystem::is_regular_file(candidate, file_error);
  if (!IsWithin(candidate, root) || !regular_file ||
      file_error || file_size > 64 * 1024 * 1024) {
    NativeLogger::Instance().Warning("[CEF] Blocked or missing liberty resource: " +
                                     request->GetURL().ToString());
    return nullptr;
  }
  const auto stream = CefStreamReader::CreateForFile(candidate.wstring());
  return stream ? new CefStreamResourceHandler(200, "OK", MimeType(candidate),
                                                SecurityHeaders(candidate), stream)
                : nullptr;
}

}  // namespace liberty
