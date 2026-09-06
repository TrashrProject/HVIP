(() => {
  'use strict';

  if (window.__PARADISE_PHONE_FRIENDS_AVATARS_V12__) return;
  window.__PARADISE_PHONE_FRIENDS_AVATARS_V12__ = '12.0.0';

  const API = '/nitro/phone-friends-api.php';
  const CACHE_MS = 60000;
  const cache = new Map();
  let busy = false;

  const keyOf = value => String(value || '').trim().toLocaleLowerCase('fr-FR');

  function friendName(row) {
    return row.querySelector('.friend-name,[class*="friend-name"]')?.textContent?.trim() || '';
  }

  function avatarUrl(look) {
    return `/avatar.php?figure=${encodeURIComponent(look)}&size=m&direction=2&head_direction=2&headonly=1&gesture=sml`;
  }

  function installAvatar(row, look, username) {
    if (!look) return;
    const url = avatarUrl(look);
    let holder = row.querySelector('.friend-avatar');

    if (holder instanceof HTMLImageElement) {
      holder.classList.add('paradise-friend-look');
      if (holder.dataset.paradiseLook !== look) {
        holder.dataset.paradiseLook = look;
        holder.src = url;
        holder.alt = username;
      }
      return;
    }

    if (!holder) {
      const nativeImage = row.querySelector('img');
      if (nativeImage instanceof HTMLImageElement) {
        nativeImage.classList.add('friend-avatar', 'paradise-friend-look');
        if (nativeImage.dataset.paradiseLook !== look) {
          nativeImage.dataset.paradiseLook = look;
          nativeImage.src = url;
          nativeImage.alt = username;
        }
        return;
      }
      return;
    }

    let image = holder.querySelector(':scope > img.paradise-friend-look');
    if (!(image instanceof HTMLImageElement)) {
      image = document.createElement('img');
      image.className = 'paradise-friend-look';
      image.loading = 'eager';
      image.decoding = 'async';
      holder.appendChild(image);
    }

    if (image.dataset.paradiseLook !== look) {
      image.dataset.paradiseLook = look;
      image.src = url;
      image.alt = username;
    }
  }

  function applyCached(rows) {
    for (const row of rows) {
      const username = friendName(row);
      if (!username) continue;
      const entry = cache.get(keyOf(username));
      if (entry?.look) installAvatar(row, entry.look, username);
    }
  }

  async function refresh() {
    const app = document.querySelector('.phone-friends-app');
    if (!app || busy) return;

    const rows = [...app.querySelectorAll('.friend-row')];
    if (!rows.length) return;

    applyCached(rows);

    const now = Date.now();
    const names = [...new Set(rows.map(friendName).filter(Boolean))];
    const stale = names.filter(name => {
      const entry = cache.get(keyOf(name));
      return !entry || now - entry.at > CACHE_MS;
    });
    if (!stale.length) return;

    busy = true;
    try {
      const params = new URLSearchParams();
      stale.slice(0, 80).forEach(name => params.append('names[]', name));
      const response = await fetch(`${API}?${params.toString()}`, {
        credentials: 'same-origin',
        cache: 'no-store'
      });
      const payload = await response.json().catch(() => null);
      if (!response.ok || !payload?.ok || !payload.users) return;

      const returned = new Map(
        Object.entries(payload.users).map(([username, look]) => [keyOf(username), String(look || '')])
      );
      const stamp = Date.now();
      stale.forEach(name => cache.set(keyOf(name), { look: returned.get(keyOf(name)) || '', at: stamp }));

      const currentRows = [...document.querySelectorAll('.phone-friends-app .friend-row')];
      applyCached(currentRows);
    } catch (error) {
      console.warn('[ParadisePhone Friends V12] avatars', error);
    } finally {
      busy = false;
    }
  }

  window.setInterval(refresh, 1600);
  window.setTimeout(refresh, 250);
  console.info('[ParadisePhone] friends V12 avatar restoration active');
})();
