interface LibertyBridge {
  emit(eventName: string, data?: unknown): void;
  on(eventName: string, callback: (data: any) => void): void;
  off(eventName: string, callback?: (data: any) => void): void;
}

interface Window {
  liberty?: LibertyBridge;
}
