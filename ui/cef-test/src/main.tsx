import { StrictMode, useEffect, useState } from "react";
import { createRoot } from "react-dom/client";
import "./style.css";

type TestResponse = { message: string; playerHealth: number; utc: string };

function App() {
  const bridge = window.liberty;
  const [message, setMessage] = useState("Hello from CEF");
  const [result, setResult] = useState(
    bridge ? "Waiting for round-trip…" : "window.liberty is unavailable"
  );

  useEffect(() => {
    const cursor = document.getElementById("liberty-cursor");
    const moveCursor = (event: PointerEvent) => {
      if (cursor) cursor.style.transform = `translate3d(${event.clientX}px, ${event.clientY}px, 0)`;
    };
    window.addEventListener("pointermove", moveCursor, { passive: true });
    return () => window.removeEventListener("pointermove", moveCursor);
  }, []);

  useEffect(() => {
    if (!bridge) return;
    const receive = (data: TestResponse) => {
      setResult(`${data.message}\nPlayer health: ${data.playerHealth}\n${data.utc}`);
    };
    bridge.on("cef:test-response", receive);
    return () => bridge.off("cef:test-response", receive);
  }, [bridge]);

  const send = () => {
    if (!bridge) return;
    setResult("Sending CEF → Native → Managed…");
    bridge.emit("cef:test", { message });
  };

  return (
    <main className="card">
      <div className="brand">PL</div>
      <p className="eyebrow">PROJECT LIBERTY CEF TEST</p>
      <h1>CEF: <span className={bridge ? "online" : "error"}>{bridge ? "ONLINE" : "BRIDGE ERROR"}</span></h1>
      <p className="meta">Browser: <strong>native OSR</strong> · UI: <strong>React + TypeScript</strong></p>
      <label>
        Test message
        <input value={message} onChange={event => setMessage(event.target.value)} />
      </label>
      <div className="actions">
        <button onClick={send} disabled={!bridge}>Send event to game</button>
        <button className="secondary" onClick={() => bridge?.emit("cef:close", {})}>Close</button>
      </div>
      <section>
        <span>Last game event</span>
        <pre>{result}</pre>
      </section>
    </main>
  );
}

createRoot(document.getElementById("root")!).render(<StrictMode><App /></StrictMode>);
