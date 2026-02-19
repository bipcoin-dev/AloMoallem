/* global signalR, lucide */
(function(){
  const bell = document.getElementById('notifBell');
  const badge = document.getElementById('notifBadge');
  const list = document.getElementById('notifList');
  if(!bell || !badge || !list) return;

  async function load(){
    try{
      const res = await fetch('/api/notifications');
      if(!res.ok) return;
      const data = await res.json();
      const unread = data.unread || 0;

      badge.style.display = unread > 0 ? 'inline-block' : 'none';
      badge.textContent = unread;

      list.innerHTML = '';
      if(!data.items || data.items.length === 0){
        const empty = document.createElement('div');
        empty.className = 'px-3 py-2 text-muted';
        empty.textContent = 'لا يوجد إشعارات.';
        list.appendChild(empty);
        return;
      }

      data.items.forEach(n=>{
        const item = document.createElement('button');
        item.type='button';
        item.className = 'dropdown-item text-wrap py-2';
        item.style.whiteSpace='normal';
        item.innerHTML = `<div class="fw-bold">${escapeHtml(n.title)}</div><div class="small text-muted">${escapeHtml(n.body)}</div>`;
        item.onclick = async ()=>{
          await fetch(`/api/notifications/${n.id}/read`, { method:'POST' });
          if(n.url) window.location.href = n.url;
          else load();
        };
        list.appendChild(item);
      });
    }catch(e){
      console.error(e);
    }
  }

  function escapeHtml(s){
    return (s||'').replace(/[&<>"']/g, m => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]));
  }

  // Realtime
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notifications')
    .withAutomaticReconnect()
    .build();

  connection.on('notify', function(n){
    // toast + reload badge
    if(typeof showToast === 'function'){
      showToast(n.title + ' — ' + n.body);
    }
    load();
  });

  connection.start().then(()=>load()).catch(()=>{});

  bell.addEventListener('click', ()=>load());

  if(window.lucide){ lucide.createIcons(); }
})();
