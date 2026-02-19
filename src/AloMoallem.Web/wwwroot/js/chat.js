/* global signalR */
(function () {
  const convoIdEl = document.getElementById("convoId");
  if (!convoIdEl) return;

  const conversationId = parseInt(convoIdEl.value, 10);
  const meEmail = (document.getElementById("meEmail")?.value || "").toLowerCase();

  const list = document.getElementById("messages");
  const input = document.getElementById("chatText");
  const sendBtn = document.getElementById("sendBtn");

  function addBubble(senderEmail, text, sentAt) {
    const mine = (senderEmail || "").toLowerCase() === meEmail;

    const wrap = document.createElement("div");
    wrap.className = "mb-2 d-flex " + (mine ? "justify-content-end" : "justify-content-start");

    const bubble = document.createElement("div");
    bubble.className = "p-2 rounded-4 border bg-white";
    bubble.style.maxWidth = "85%";

    const meta = document.createElement("div");
    meta.className = "small text-muted mb-1";
    meta.textContent = senderEmail || "";

    const body = document.createElement("div");
    body.className = "fw-semibold";
    body.textContent = text;

    const time = document.createElement("div");
    time.className = "small text-muted mt-1";
    time.textContent = sentAt ? new Date(sentAt).toLocaleString() : "";

    bubble.appendChild(meta);
    bubble.appendChild(body);
    bubble.appendChild(time);
    wrap.appendChild(bubble);
    list.appendChild(wrap);

    // scroll bottom
    list.parentElement.scrollTop = list.parentElement.scrollHeight;
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect()
    .build();

  connection.on("message", function (m) {
    if (m.conversationId !== conversationId) return;
    addBubble(m.senderEmail, m.text, m.sentAtUtc);
  });

  async function start() {
    try {
      await connection.start();
      await connection.invoke("JoinConversation", conversationId);
      document.getElementById("rtStatus").textContent = "متصل";
    } catch (e) {
      document.getElementById("rtStatus").textContent = "غير متصل";
      console.error(e);
      setTimeout(start, 1500);
    }
  }

  async function send() {
    const t = (input.value || "").trim();
    if (!t) return;
    input.value = "";
    input.focus();
    await connection.invoke("SendMessage", conversationId, t);
  }

  sendBtn?.addEventListener("click", function (e) {
    e.preventDefault();
    send();
  });

  input?.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      send();
    }
  });

  start();
})();
