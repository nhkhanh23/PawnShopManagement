(() => {
 document.addEventListener('submit', e => { const message=e.target.dataset.confirm; if(message&&!window.confirm(message)) e.preventDefault(); });
 const form=document.querySelector('#contract-filter'), results=document.querySelector('#results');
 let pending;
 async function load(url,push=true) {
   if(pending) pending.abort(); pending=new AbortController(); results.classList.add('is-loading'); results.setAttribute('aria-busy','true');
   document.querySelector('.ajax-error')?.remove();
   try {
     const response=await fetch(url,{headers:{'X-Requested-With':'XMLHttpRequest'},signal:pending.signal});
     if(response.redirected) { location.assign(response.url); return; }
     if(!response.ok) throw new Error('request');
     results.innerHTML=await response.text(); if(push) history.pushState({},'',url);
   } catch(error) { if(error.name!=='AbortError') { const p=document.createElement('p'); p.className='ajax-error'; p.setAttribute('role','alert'); p.textContent=document.documentElement.lang==='en'?'Could not load data. Please try again.':'Không tải được dữ liệu. Vui lòng thử lại.'; results.before(p); } }
   finally { results.classList.remove('is-loading'); results.removeAttribute('aria-busy'); }
 }
 if(form&&results) {
   form.addEventListener('submit',e=>{e.preventDefault();const url=new URL(form.action);url.search=new URLSearchParams(new FormData(form)).toString();load(url.href);});
   results.addEventListener('click',e=>{const link=e.target.closest('a[data-page]');if(link){e.preventDefault();load(link.href);}});
   window.addEventListener('popstate',()=>{const params=new URLSearchParams(location.search);form.elements.searchString.value=params.get('searchString')||'';form.elements.statusFilter.value=params.get('statusFilter')||'';form.elements.deadlineFilter.value=params.get('deadlineFilter')||'';load(location.href,false);});
 }
 const start=document.querySelector('#StartDate'), term=document.querySelector('#TermDays'), due=document.querySelector('#due-date-preview');
 if(start&&term&&due) { const updateDue=()=>{const days=Number(term.value);const date=new Date(start.value+'T00:00:00Z');if(!Number.isInteger(days)||days<1||days>3650||!start.value||!Number.isFinite(date.getTime())){due.textContent=document.documentElement.lang==='en'?'Not specified':'Chưa xác định';return;}date.setUTCDate(date.getUTCDate()+days);due.textContent=new Intl.DateTimeFormat(document.documentElement.lang==='en'?'en-GB':'vi-VN',{timeZone:'UTC'}).format(date);};start.addEventListener('input',updateDue);term.addEventListener('input',updateDue);updateDue(); }
 const amount=document.querySelector('#LoanAmount'),rate=document.querySelector('#InterestRate'),preview=document.querySelector('#interest-preview');
 if(amount&&rate&&preview) {const update=()=>{ const value=Number(amount.value)*Number(rate.value)/100;preview.textContent=new Intl.NumberFormat(document.documentElement.lang==='en'?'en-US':'vi-VN',{maximumFractionDigits:0}).format(Number.isFinite(value)?value:0)+' đ'; };amount.addEventListener('input',update);rate.addEventListener('input',update);update();}
})();
