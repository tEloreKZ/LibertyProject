#pragma once

#include <cstdint>

#if defined(PROJECTLIBERTY_CORE_EXPORTS)
#define PL_API extern "C" __declspec(dllexport)
#else
#define PL_API extern "C" __declspec(dllimport)
#endif

using LibertyBrowserId = std::uint64_t;

enum LibertyResult : std::int32_t {
  LibertyResult_Ok = 0,
  LibertyResult_NotReady = 1,
  LibertyResult_InvalidArgument = 2,
  LibertyResult_NotFound = 3,
  LibertyResult_BufferTooSmall = 4,
  LibertyResult_InternalError = 5,
};

struct LibertyBrowserOptions {
  std::uint32_t struct_size;
  const char* url_utf8;
  const char* owner_utf8;
  std::int32_t width;
  std::int32_t height;
  std::int32_t x;
  std::int32_t y;
  std::int32_t fps;
  std::int32_t z_index;
  std::uint8_t transparent;
  std::uint8_t visible;
  std::uint8_t focused;
  std::uint8_t reserved;
};

struct LibertyBrowserEventInfo {
  std::uint32_t struct_size;
  LibertyBrowserId browser_id;
  std::uint32_t event_name_bytes;
  std::uint32_t json_bytes;
};

PL_API std::uint32_t Liberty_GetApiVersion();
PL_API std::int32_t Liberty_IsReady();
PL_API LibertyResult Liberty_CreateBrowser(const LibertyBrowserOptions* options,
                                           LibertyBrowserId* browser_id);
PL_API LibertyResult Liberty_DestroyBrowser(LibertyBrowserId browser_id);
PL_API LibertyResult Liberty_DestroyBrowsersByOwner(const char* owner_utf8);
PL_API LibertyResult Liberty_ShowBrowser(LibertyBrowserId browser_id);
PL_API LibertyResult Liberty_HideBrowser(LibertyBrowserId browser_id);
PL_API LibertyResult Liberty_FocusBrowser(LibertyBrowserId browser_id);
PL_API LibertyResult Liberty_UnfocusBrowser(LibertyBrowserId browser_id);
PL_API LibertyResult Liberty_LoadBrowserUrl(LibertyBrowserId browser_id,
                                            const char* url_utf8);
PL_API LibertyResult Liberty_SetBrowserBounds(LibertyBrowserId browser_id,
                                              std::int32_t x,
                                              std::int32_t y,
                                              std::int32_t width,
                                              std::int32_t height);
PL_API LibertyResult Liberty_SetBrowserZIndex(LibertyBrowserId browser_id,
                                              std::int32_t z_index);
PL_API LibertyResult Liberty_BrowserEmit(LibertyBrowserId browser_id,
                                         const char* event_name_utf8,
                                         const char* json_utf8);
PL_API LibertyResult Liberty_PeekBrowserEvent(LibertyBrowserEventInfo* info);
PL_API LibertyResult Liberty_PollBrowserEvent(char* event_name_utf8,
                                              std::uint32_t event_name_capacity,
                                              char* json_utf8,
                                              std::uint32_t json_capacity,
                                              LibertyBrowserEventInfo* info);

