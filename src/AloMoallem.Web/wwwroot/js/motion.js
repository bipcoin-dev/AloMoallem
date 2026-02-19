/*
  Mu3allem Motion System (lightweight)
  - data-motion="fade" | "slide-up" | "scale"
  - data-stagger="80" على container
  - IntersectionObserver triggers once
*/
(function(){
  const prefersReduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  if(prefersReduce) return;

  const ease = 'cubic-bezier(.2,.8,.2,1)';

  function apply(el, i, baseDelay){
    const type = el.dataset.motion || 'fade';
    const delay = (baseDelay || 0) + i * (parseInt(el.closest('[data-stagger]')?.dataset.stagger || '0',10) || 0);

    el.style.transition = `opacity .35s ${ease} ${delay}ms, transform .35s ${ease} ${delay}ms`;
    el.style.willChange = 'opacity, transform';

    if(type === 'slide-up'){ el.style.opacity='0'; el.style.transform='translateY(14px)'; }
    else if(type === 'scale'){ el.style.opacity='0'; el.style.transform='scale(.98)'; }
    else { el.style.opacity='0'; el.style.transform='translateY(6px)'; }

    // trigger later
    requestAnimationFrame(()=>{
      el.style.opacity='1';
      el.style.transform='none';
    });
  }

  const els = Array.from(document.querySelectorAll('[data-motion]'));
  const io = new IntersectionObserver((entries)=>{
    entries.forEach(entry=>{
      if(!entry.isIntersecting) return;
      const target = entry.target;
      const group = target.closest('[data-stagger]');
      if(group && group.__done) { io.unobserve(target); return; }

      if(group && !group.__done){
        group.__done = true;
        const items = Array.from(group.querySelectorAll('[data-motion]'));
        items.forEach((el, idx)=>apply(el, idx, 0));
        items.forEach(el=>io.unobserve(el));
        return;
      }

      apply(target, 0, 0);
      io.unobserve(target);
    });
  }, { threshold: 0.15 });

  els.forEach(el=>io.observe(el));
})();
