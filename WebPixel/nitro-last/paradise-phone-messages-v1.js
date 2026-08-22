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
      triggger.textContent = '+';
      const slot = header.lastElementChild;
      if (slot?.tagName === 'SPAN' && !clean(slot.textContent))