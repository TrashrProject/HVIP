(() => {
  'use strict';

  if (window.ParadisePhoneMessagesV1) return;

  const VERSION = '1.0.0-messages-v1';
  const ACTION_URL = '../rp-phone-action.php';
  const HUD_SELECTOR = '#paradise-rp-hud';

  let destroyed = false;
  let scheduled = false;
  let observer = null;
  let newConversationOpen = false;
  let selectedContactNumber = null;
  let sendPending = false;
  let syncPending = false;
  let activeConversationId = null;
  let activeConversationSignature = '';
  let cachedConversationId = null;
  let cachedMessages = null;
  let cachedMessagesSignature = '';

  const phoneRoot = () => document.querySelector(`${HUD_SELECTOR} .pp-device`);
  const phoneContent = () => phoneRoot()?.querySelector('[data-pp-content]') || null;
  const phoneState = () => window.ParadiseStore?.getState?.().phone || {};
  const clean = value => value == null ? '' : String(value).trim();
  const safeLook = value => /^[a-z0-9.\-]+$/i.test(clean(value));

  function parseDate(value) {
    const raw = clean(value);
    if (!raw) return null;
    const candidate = raw.includes('T') ? raw : raw.replace(' ', 'T');
    const date = new Date(candidate);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  function formatListDate(value) {
    const date = parseDate(value);
    if (!date) return '';
    const now = new Date();
    const startToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const startDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const days = Math.round((startToday - startDate) / 86400000);
    if (days === 0) return new Intl.DateTimeFormat('fr-FR', { hour:'2-digit', minute:'2-digit' }).format(date);
    if (days === 1) return 'Hier';
    if (date.getFullYear() === now.getFullYear()) return new Intl.DateTimeFormat('fr-FR', { day:'2-digit', month:'2-digit' }).format(date);
    return new Intl.DateTimeFormat('fr-FR', { day:'2-digit', month:'2-digit', year:'2-digit' }).format(date);
  }

  function formatMessageTime(value) {
    const date = parseDate(value);
    if (!date) return '';
    return new Intl.DateTimeFormat('fr-FR', { hour:'2-digit', minute:'2-digit' }).format(date);
  }

  function avatarNode(look, name, size = 'm') {
    const frame = document.createElement('span');
    frame.className = 'pp-avatar pp-msg-avatar';
    if (safeLook(look)) {
      const img = document.createElement('img');
      img.src = `../avatar-image.php?figure=${encodeURIComponent(clean(look))}&direction=2&head_direction=3&gesture=sml&action=std&size=${encodeURIComponent(size)}&phone=messages-v1`;
      img.alt = clean(name) || 'Contact';
      img.draggable = false;
      frame.appendChild(img);
    } else {
      const fallback = document.createElement('span');
      fallback.className = 'pp-avatar-fallback';
      fallback.textContent = (clean(name) || 'P').slice(0, 1).toUpperCase();
      frame.appendChild(fallback);
    }
    return frame;
  }

  async function phoneAction(payload) {
    const response = await fetch(ACTION_URL, {
      method: 'POST',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
        'X-Paradise-Action': 'phase4'
      },
      body: JSON.stringify(payload || {})
    });
    let data = null;
    try { data = await response.json(); } catch (_) {}
    if (!response.ok || !data?.ok) {
      const error = new Error(data?.message || data?.reason || `HTTP ${response.status}`);
      error.payload = data;
      throw error;
    }
    return data;
  }

  function findMessagesPage() {
    return [...(phoneRoot()?.querySelectorAll('.pp-app-page') || [])].find(node =>
      clean(node.querySelector(':scope > header strong')?.textContent) === 'Messages'
    ) || null;
  }

  function findConversationByNumber(number) {
    const target = clean(number);
    return (phoneState().conversations || []).find(row => clean(row?.number) === target) || null;
  }

  function findContactByNumber(number) {
    const target = clean(number);
    return (phoneState().contacts || []).find(row => clean(row?.number) === target) || null;
  }

  function conversationSignature(row) {
    if (!row) return '';
    return [row.phone_id, row.last_at, row.last_message, row.unread].map(v => String(v ?? '')).join('|');
  }

  function messagesSignature(messages) {
    return (messages || []).map(message => [message.id, message.mine ? 1 : 0, message.sent_at, message.read_at, message.body].join(':')).join('|');
  }

  function ensureListHeader(page) {
    page.dataset.ppMessagesV1 = 'list';
    page.classList.add('pp-messages-app');
    const header = page.querySelector(':scope > header');
    if (!header) return;
    header.classList.add('pp-msg-header');
    const title = header.querySelector('strong');
    if (title) title.textContent = 'Messages';
    const back = header.querySelector('.pp-back');
    if (back) {
      back.setAttribute('aria-label', 'Retour à l’accueil');
      back.title = 'Accueil';
    }

    let trigger = header.querySelector('[data-pp-new-message-trigger]');
    if (!trigger) {
      trigger = document.createElement('button');
      trigger.type = 'button';
      trigger.className = 'pp-msg-new-trigger';
      trigger.dataset.ppNewMessageTrigger = '1';
      trigger.setAttribute('aria-label', 'Nouvelle conversation');
      trigger.title = 'Nouvelle conversation';
      trigger.textContent = '+';
      const slot = header.lastElementChild;
      if (slot?.tagName === 'SPAN' && !clean(slot.textContent)) slot.replaceWith(trigger);
      else header.appendChild(trigger);
    } else {
      trigger.classList.add('pp-msg-new-trigger');
      trigger.setAttribute('aria-label', 'Nouvelle conversation');
      trigger.title = 'Nouvelle conversation';
    }
  }

  function enhanceConversationRows(page) {
    const byId = new Map((phoneState().conversations || []).map(row => [String(row.phone_id), row]));
    page.querySelectorAll('.pp-list [data-pp-conversation]').forEach(node => {
      const row = byId.get(String(node.dataset.ppConversation));
      if (!row) return;
      node.classList.add('pp-msg-conversation-row');
      node.classList.toggle('is-unread', Number(row.unread || 0) > 0);
      const meta = node.querySelector('.pp-row-meta time');
      if (meta) {
        meta.textContent = formatListDate(row.last_at);
        meta.dateTime = clean(row.last_at);
      }
      const preview = node.querySelector('.pp-row-main small');
      if (preview) preview.title = clean(row.last_message);
      const name = node.querySelector('.pp-row-main strong');
      if (name) name.textContent = clean(row.name) || clean(row.number);
    });
  }

  function simplifyEmptyState(page) {
    const empty = page.querySelector('.pp-empty');
    if (!empty) return;
    empty.classList.add('pp-msg-empty');
    empty.querySelectorAll('.ppp-empty-art,[data-pp-empty-new-message]').forEach(node => node.remove());
    const strong = empty.querySelector('strong');
    const text = empty.querySelector('span');
    if (strong) strong.textContent = 'Aucune conversation';
    if (text) text.textContent = 'Commencez une discussion avec un contact.';
  }

  function createNewScreen() {
    const screen = document.createElement('section');
    screen.className = 'pp-msg-new-screen pp-chat';
    screen.dataset.ppMessagesV1 = 'new';
    screen.innerHTML = `
      <header class="pp-msg-header pp-msg-new-header">
        <button type="button" class="pp-back" data-pp-app="messages" data-pp-msg-new-back aria-label="Retour">‹</button>
        <strong>Nouvelle discussion</strong>
        <span></span>
      </header>
      <div class="pp-msg-new-body" data-pp-msg-new-body></div>`;
    return screen;
  }

  function renderContactPicker(container) {
    const contacts = phoneState().contacts || [];
    container.replaceChildren();
    const intro = document.createElement('div');
    intro.className = 'pp-msg-section-label';
    intro.textContent = 'CONTACTS';
    container.appendChild(intro);

    if (!contacts.length) {
      const empty = document.createElement('div');
      empty.className = 'pp-msg-new-empty';
      const title = document.createElement('strong');
      title.textContent = 'Aucun contact';
      const subtitle = document.createElement('span');
      subtitle.textContent = 'Ajoutez d’abord un contact dans ParadisePhone.';
      empty.append(title, subtitle);
      container.appendChild(empty);
      return;
    }

    const list = document.createElement('div');
    list.className = 'pp-msg-contact-list';
    contacts.forEach(contact => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'pp-msg-contact-row';
      button.dataset.ppMsgContactNumber = clean(contact.number);
      button.appendChild(avatarNode(contact.look, contact.name));
      const copy = document.createElement('span');
      copy.className = 'pp-msg-contact-copy';
      const name = document.createElement('strong');
      name.textContent = clean(contact.name) || clean(contact.number);
      const number = document.createElement('small');
      number.textContent = clean(contact.number);
      copy.append(name, number);
      const arrow = document.createElement('span');
      arrow.className = 'pp-msg-contact-arrow';
      arrow.textContent = '›';
      button.append(copy, arrow);
      list.appendChild(button);
    });
    container.appendChild(list);
  }

  function renderNewComposer(container, contact) {
    container.replaceChildren();
    const recipient = document.createElement('div');
    recipient.className = 'pp-msg-recipient';
    recipient.appendChild(avatarNode(contact?.look, contact?.name));
    const copy = document.createElement('div');
    const title = document.createElement('strong');
    title.textContent = clean(contact?.name) || clean(contact?.number) || 'Contact';
    const number = document.createElement('small');
    number.textContent = clean(contact?.number);
    copy.append(title, number);
    recipient.appendChild(copy);

    const hint = document.createElement('p');
    hint.className = 'pp-msg-first-hint';
    hint.textContent = 'Commencez la conversation.';

    const form = document.createElement('form');
    form.className = 'pp-msg-new-compose';
    form.dataset.ppMsgNewCompose = '1';
    const input = document.createElement('textarea');
    input.rows = 1;
    input.maxLength = 500;
    input.placeholder = 'Écrire un message...';
    input.dataset.ppMsgNewInput = '1';
    input.setAttribute('aria-label', 'Message');
    const send = document.createElement('button');
    send.type = 'submit';
    send.className = 'pp-msg-send';
    send.setAttribute('aria-label', 'Envoyer');
    send.textContent = '➤';
    form.append(input, send);
    container.append(recipient, hint, form);
    requestAnimationFrame(() => input.focus());
  }

  function renderNewScreen() {
    const content = phoneContent();
    const page = findMessagesPage();
    if (!content || !page || !newConversationOpen) return;
    page.hidden = true;
    let screen = content.querySelector('.pp-msg-new-screen');
    if (!screen) {
      screen = createNewScreen();
      content.appendChild(screen);
    }
    const body = screen.querySelector('[data-pp-msg-new-body]');
    if (!body) return;
    const contact = selectedContactNumber ? findContactByNumber(selectedContactNumber) : null;
    const renderedFor = body.dataset.renderedFor || '';
    const targetKey = contact ? clean(contact.number) : '__picker__';
    if (renderedFor === targetKey) return;
    body.dataset.renderedFor = targetKey;
    if (contact) renderNewComposer(body, contact);
    else renderContactPicker(body);
  }

  function closeNewScreen() {
    newConversationOpen = false;
    selectedContactNumber = null;
    phoneContent()?.querySelector('.pp-msg-new-screen')?.remove();
    const page = findMessagesPage();
    if (page) page.hidden = false;
    schedule();
  }

  function openNewScreen() {
    newConversationOpen = true;
    selectedContactNumber = null;
    renderNewScreen();
  }

  function openExistingConversation(conversationId) {
    closeNewScreen();
    window.ParadisePhoneV1?.open?.('messages');
    requestAnimationFrame(() => requestAnimationFrame(() => {
      const row = phoneRoot()?.querySelector(`[data-pp-conversation="${CSS.escape(String(conversationId))}"]`);
      row?.click();
    }));
  }

  function renderMessagesIntoChat(chat, messages) {
    const host = chat?.querySelector('[data-pp-messages]');
    if (!host) return;
    const signature = messagesSignature(messages);
    if (host.dataset.ppMsgSignature === signature) return;
    host.dataset.ppMsgSignature = signature;
    host.replaceChildren();
    (messages || []).forEach(message => {
      const bubble = document.createElement('div');
      bubble.className = `pp-bubble ${message.mine ? 'is-mine' : 'is-theirs'}`;
      const body = document.createElement('span');
      body.textContent = String(message.body ?? '');
      const time = document.createElement('time');
      time.textContent = formatMessageTime(message.sent_at);
      if (message.sent_at) time.dateTime = String(message.sent_at);
      bubble.append(body, time);
      host.appendChild(bubble);
    });
    requestAnimationFrame(() => { host.scrollTop = host.scrollHeight; });
  }

  async function syncOpenConversation(row) {
    if (syncPending || !row?.phone_id) return;
    syncPending = true;
    try {
      const history = await phoneAction({ action:'conversation', other_phone_id:Number(row.phone_id) });
      const messages = Array.isArray(history.messages) ? history.messages : [];
      cachedConversationId = Number(row.phone_id);
      cachedMessages = messages;
      cachedMessagesSignature = messagesSignature(messages);
      const chat = phoneRoot()?.querySelector('.pp-chat:not(.pp-msg-new-screen)');
      if (chat) renderMessagesIntoChat(chat, messages);
      if (Number(row.unread || 0) > 0) {
        try { await phoneAction({ action:'read_conversation', other_phone_id:Number(row.phone_id) }); } catch (_) {}
      }
    } catch (error) {
      console.warn('[ParadiseRP:MessagesV1] conversation sync failed', error?.message || error);
    } finally {
      syncPending = false;
    }
  }

  function enhanceChat(chat) {
    if (!chat || chat.classList.contains('pp-msg-new-screen')) return;
    chat.dataset.ppMessagesV1 = 'chat';
    chat.classList.add('pp-messages-chat');
    phoneRoot()?.classList.add('pp-msg-mode');
    const header = chat.querySelector(':scope > header');
    header?.classList.add('pp-msg-chat-header');
    const call = header?.querySelector('[data-pp-call-number]');
    if (call) call.hidden = true;
    const back = header?.querySelector('[data-pp-app="messages"]');
    if (back) {
      back.setAttribute('aria-label', 'Retour aux messages');
      back.title = 'Messages';
    }

    const number = clean(header?.querySelector('small')?.textContent);
    const row = findConversationByNumber(number);
    if (!row) return;
    const id = Number(row.phone_id);
    const signature = conversationSignature(row);
    if (activeConversationId !== id) {
      activeConversationId = id;
      activeConversationSignature = signature;
      cachedConversationId = null;
      cachedMessages = null;
      cachedMessagesSignature = '';
    } else if (signature !== activeConversationSignature) {
      activeConversationSignature = signature;
      cachedMessages = null;
      cachedMessagesSignature = '';
      syncOpenConversation(row);
    } else if (cachedConversationId === id && cachedMessages) {
      renderMessagesIntoChat(chat, cachedMessages);
    }
  }

  function enhanceList(page) {
    phoneRoot()?.classList.add('pp-msg-mode');
    ensureListHeader(page);
    enhanceConversationRows(page);
    simplifyEmptyState(page);
    page.querySelectorAll('[data-pp-new-message-sheet]').forEach(sheet => { sheet.hidden = true; sheet.setAttribute('aria-hidden', 'true'); });
    if (newConversationOpen) renderNewScreen();
    activeConversationId = null;
    activeConversationSignature = '';
    cachedConversationId = null;
    cachedMessages = null;
    cachedMessagesSignature = '';
  }

  function enhance() {
    const root = phoneRoot();
    if (!root) return;
    const page = findMessagesPage();
    const chat = root.querySelector('.pp-chat:not(.pp-msg-new-screen)');
    if (page) enhanceList(page);
    else if (chat) enhanceChat(chat);
    else {
      root.classList.remove('pp-msg-mode');
      newConversationOpen = false;
      selectedContactNumber = null;
      activeConversationId = null;
      activeConversationSignature = '';
      cachedConversationId = null;
      cachedMessages = null;
      cachedMessagesSignature = '';
    }
  }

  function schedule() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(() => {
      scheduled = false;
      if (!destroyed) enhance();
    }));
  }

  async function sendFirstMessage(form) {
    if (sendPending) return;
    const contact = findContactByNumber(selectedContactNumber);
    const input = form.querySelector('[data-pp-msg-new-input]');
    const message = clean(input?.value);
    if (!contact || !message) return;
    sendPending = true;
    const button = form.querySelector('button[type="submit"]');
    if (button) button.disabled = true;
    try {
      await window.ParadisePhoneV1?.send?.(contact.number, message);
      if (input) input.value = '';
      await window.ParadisePhoneV1?.refresh?.();
      const conversation = findConversationByNumber(contact.number);
      if (conversation?.phone_id) openExistingConversation(conversation.phone_id);
      else {
        const body = form.closest('[data-pp-msg-new-body]');
        if (body) body.dataset.renderedFor = '';
        schedule();
      }
    } catch (error) {
      const body = form.closest('[data-pp-msg-new-body]');
      if (body) {
        let errorNode = body.querySelector('.pp-msg-inline-error');
        if (!errorNode) {
          errorNode = document.createElement('div');
          errorNode.className = 'pp-msg-inline-error';
          body.insertBefore(errorNode, form);
        }
        errorNode.textContent = error?.message || 'Envoi impossible.';
      }
    } finally {
      sendPending = false;
      if (button?.isConnected) button.disabled = false;
    }
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;

    const newTrigger = target.closest('[data-pp-new-message-trigger]');
    if (newTrigger && phoneRoot()?.contains(newTrigger)) {
      event.preventDefault();
      event.stopPropagation();
      event.stopImmediatePropagation();
      openNewScreen();
      return;
    }

    const newBack = target.closest('[data-pp-msg-new-back]');
    if (newBack && phoneRoot()?.contains(newBack)) {
      event.preventDefault();
      event.stopPropagation();
      event.stopImmediatePropagation();
      closeNewScreen();
      return;
    }

    const contactRow = target.closest('[data-pp-msg-contact-number]');
    if (contactRow && phoneRoot()?.contains(contactRow)) {
      event.preventDefault();
      event.stopPropagation();
      event.stopImmediatePropagation();
      const number = clean(contactRow.dataset.ppMsgContactNumber);
      const existing = findConversationByNumber(number);
      if (existing?.phone_id) openExistingConversation(existing.phone_id);
      else {
        selectedContactNumber = number;
        const body = phoneContent()?.querySelector('[data-pp-msg-new-body]');
        if (body) body.dataset.renderedFor = '';
        renderNewScreen();
      }
    }
  }

  function onSubmit(event) {
    const form = event.target.closest?.('[data-pp-msg-new-compose]');
    if (!form || !phoneRoot()?.contains(form)) return;
    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();
    sendFirstMessage(form);
  }

  function onKeyDown(event) {
    const target = event.target;
    if (!(target instanceof HTMLInputElement) && !(target instanceof HTMLTextAreaElement)) return;
    if (!target.closest(`${HUD_SELECTOR} .pp-device`)) return;
    if (!target.closest('[data-pp-messages-v1],.pp-messages-chat')) return;

    if (event.key === 'Enter' && !event.shiftKey && (target.matches('[data-pp-message-input]') || target.matches('[data-pp-msg-new-input]'))) {
      event.preventDefault();
      event.stopPropagation();
      event.stopImmediatePropagation();
      target.closest('form')?.requestSubmit();
      return;
    }
    event.stopPropagation();
  }

  function onKeyUp(event) {
    const target = event.target;
    if (!(target instanceof HTMLInputElement) && !(target instanceof HTMLTextAreaElement)) return;
    if (target.closest(`${HUD_SELECTOR} .pp-device [data-pp-messages-v1],${HUD_SELECTOR} .pp-device .pp-messages-chat`)) event.stopPropagation();
  }

  function onPhoneUpdate() {
    schedule();
    requestAnimationFrame(() => {
      const chat = phoneRoot()?.querySelector('.pp-chat:not(.pp-msg-new-screen)');
      if (!chat) return;
      const number = clean(chat.querySelector('header small')?.textContent);
      const row = findConversationByNumber(number);
      if (!row) return;
      const signature = conversationSignature(row);
      if (activeConversationId === Number(row.phone_id) && signature !== activeConversationSignature) {
        activeConversationSignature = signature;
        cachedMessages = null;
        cachedMessagesSignature = '';
        syncOpenConversation(row);
      }
    });
  }

  function observePhone() {
    observer?.disconnect();
    const root = phoneRoot();
    const content = root?.querySelector('[data-pp-content]');
    if (!content) return;
    observer = new MutationObserver(schedule);
    observer.observe(content, { childList:true, subtree:true });
  }

  function boot() {
    document.addEventListener('click', onClick, true);
    document.addEventListener('submit', onSubmit, true);
    document.addEventListener('keydown', onKeyDown, true);
    document.addEventListener('keyup', onKeyUp, true);
    window.addEventListener('paradise:phone', onPhoneUpdate, false);
    observePhone();
    schedule();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    observer?.disconnect();
    observer = null;
    document.removeEventListener('click', onClick, true);
    document.removeEventListener('submit', onSubmit, true);
    document.removeEventListener('keydown', onKeyDown, true);
    document.removeEventListener('keyup', onKeyUp, true);
    window.removeEventListener('paradise:phone', onPhoneUpdate, false);
    phoneRoot()?.classList.remove('pp-msg-mode');
  }

  window.ParadisePhoneMessagesV1 = Object.freeze({
    version: VERSION,
    refresh: schedule,
    destroy,
    getStatus: () => ({
      version: VERSION,
      mounted: Boolean(phoneRoot()),
      list: Boolean(findMessagesPage()),
      chat: Boolean(phoneRoot()?.querySelector('.pp-messages-chat')),
      newConversation: newConversationOpen,
      selectedContact: selectedContactNumber,
      existingTransportPollingMs: 2200,
      extraPolling: false,
      syncPending,
      sendPending
    })
  });

  window.addEventListener('beforeunload', destroy, { once:true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once:true });
  else boot();
})();
