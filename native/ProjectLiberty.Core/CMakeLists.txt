cmake_minimum_required(VERSION 3.21)

project(ProjectLibertyNative VERSION 0.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CONFIGURATION_TYPES Debug Release)
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

if(NOT CEF_ROOT)
  set(CEF_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../../third_party/cef" CACHE PATH "CEF binary distribution")
endif()
get_filename_component(CEF_ROOT "${CEF_ROOT}" ABSOLUTE)
if(NOT EXISTS "${CEF_ROOT}/include/cef_app.h")
  message(FATAL_ERROR "CEF was not found at ${CEF_ROOT}. Run Build/Setup-Cef.ps1 first.")
endif()

list(APPEND CMAKE_MODULE_PATH "${CEF_ROOT}/cmake")
find_package(CEF REQUIRED)
add_subdirectory("${CEF_LIBCEF_DLL_WRAPPER_PATH}" libcef_dll_wrapper)

include(FetchContent)
FetchContent_Declare(
  minhook
  URL https://github.com/TsudaKageyu/minhook/archive/refs/tags/v1.3.4.tar.gz
  URL_HASH SHA256=1AEBEAE4CA898330C507860ACC2FCA2EB335FE446A3A2B8444C3BF8B2660A14E
  DOWNLOAD_EXTRACT_TIMESTAMP TRUE
)
FetchContent_MakeAvailable(minhook)
if(TARGET minhook)
  set_property(TARGET minhook PROPERTY
    MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

if(NOT EXISTS "${CEF_LIB_DEBUG}")
  set(CEF_LIB_DEBUG "${CEF_LIB_RELEASE}")
endif()
ADD_LOGICAL_TARGET(libcef_lib "${CEF_LIB_DEBUG}" "${CEF_LIB_RELEASE}")

set(PL_NATIVE_SOURCES
  src/Browser.cpp
  src/BrowserManager.cpp
  src/CoreRuntime.cpp
  src/EventQueue.cpp
  src/Exports.cpp
  src/InputManager.cpp
  src/Logging/NativeLogger.cpp
  src/Cef/CefApp.cpp
  src/Cef/CefClient.cpp
  src/Cef/CefRenderHandler.cpp
  src/Cef/CefRuntime.cpp
  src/Cef/CefSchemeHandler.cpp
  src/Cef/CefV8Handler.cpp
  src/Graphics/D3D11Backend.cpp
  src/Graphics/DxgiHook.cpp
)

add_library(ProjectLibertyCore SHARED ${PL_NATIVE_SOURCES})
set_target_properties(ProjectLibertyCore PROPERTIES
  OUTPUT_NAME "ProjectLiberty.Core"
  SUFFIX ".asi"
  PREFIX ""
  FOLDER "ProjectLiberty"
)
target_compile_definitions(ProjectLibertyCore PRIVATE NOMINMAX WIN32_LEAN_AND_MEAN PROJECTLIBERTY_CORE_EXPORTS)
target_include_directories(ProjectLibertyCore PRIVATE include src)
target_include_directories(ProjectLibertyCore SYSTEM PRIVATE "${CEF_ROOT}")
target_link_libraries(ProjectLibertyCore PRIVATE
  libcef_lib libcef_dll_wrapper minhook
  d3d11 d3d12 dxgi d3dcompiler delayimp shlwapi user32
)
target_link_options(ProjectLibertyCore PRIVATE "/DELAYLOAD:libcef.dll")

add_library(ProjectLibertyCefSubprocess SHARED
  src/SubprocessMain.cpp
  src/Cef/CefApp.cpp
  src/Cef/CefV8Handler.cpp
)
set_target_properties(ProjectLibertyCefSubprocess PROPERTIES
  OUTPUT_NAME "ProjectLiberty.CefSubprocess"
  PREFIX ""
  FOLDER "ProjectLiberty"
)
SET_LIBRARY_TARGET_PROPERTIES(ProjectLibertyCefSubprocess)
target_include_directories(ProjectLibertyCefSubprocess PRIVATE include src)
target_include_directories(ProjectLibertyCefSubprocess SYSTEM PRIVATE "${CEF_ROOT}")
target_link_libraries(ProjectLibertyCefSubprocess PRIVATE
  libcef_lib libcef_dll_wrapper ${CEF_STANDARD_LIBS}
)

foreach(target ProjectLibertyCore ProjectLibertyCefSubprocess)
  set_property(TARGET ${target} PROPERTY
    MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
  if(MSVC)
    target_compile_options(${target} PRIVATE /W4 /permissive- /utf-8 /external:W0)
  endif()
endforeach()

set(PL_NATIVE_OUT "${CMAKE_BINARY_DIR}/deploy")
add_custom_command(TARGET ProjectLibertyCore POST_BUILD
  COMMAND ${CMAKE_COMMAND} -E make_directory "${PL_NATIVE_OUT}"
  COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE:ProjectLibertyCore>" "${PL_NATIVE_OUT}/ProjectLiberty.Core.asi"
)
add_custom_command(TARGET ProjectLibertyCefSubprocess POST_BUILD
  COMMAND ${CMAKE_COMMAND} -E remove_directory "${PL_NATIVE_OUT}/ProjectLiberty"
  COMMAND ${CMAKE_COMMAND} -E remove_directory "${PL_NATIVE_OUT}/scripts/ProjectLiberty/Native"
  COMMAND ${CMAKE_COMMAND} -E make_directory "${PL_NATIVE_OUT}/scripts/ProjectLiberty/Native"
  COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE:ProjectLibertyCefSubprocess>" "${PL_NATIVE_OUT}/scripts/ProjectLiberty/Native/ProjectLiberty.CefSubprocess.dll"
  COMMAND ${CMAKE_COMMAND} -E copy "${CEF_ROOT}/Release/bootstrap.exe" "${PL_NATIVE_OUT}/scripts/ProjectLiberty/Native/ProjectLiberty.CefSubprocess.exe"
  COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "${CEF_ROOT}/Release/libcef.dll"
    "${CEF_ROOT}/Release/chrome_elf.dll"
    "${CEF_ROOT}/Release/d3dcompiler_47.dll"
    "${CEF_ROOT}/Release/dxcompiler.dll"
    "${CEF_ROOT}/Release/dxil.dll"
    "${CEF_ROOT}/Release/libEGL.dll"
    "${CEF_ROOT}/Release/libGLESv2.dll"
    "${CEF_ROOT}/Release/v8_context_snapshot.bin"
    "${CEF_ROOT}/Release/vk_swiftshader.dll"
    "${CEF_ROOT}/Release/vk_swiftshader_icd.json"
    "${CEF_ROOT}/Release/vulkan-1.dll"
    "${PL_NATIVE_OUT}/scripts/ProjectLiberty/Native"
  COMMAND ${CMAKE_COMMAND} -E copy_directory "${CEF_ROOT}/Resources" "${PL_NATIVE_OUT}/scripts/ProjectLiberty/Native"
)

add_dependencies(ProjectLibertyCore ProjectLibertyCefSubprocess)
