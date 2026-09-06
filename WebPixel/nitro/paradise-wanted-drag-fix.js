(() => {
  'use strict';

  const SHELL_SELECTOR = '.nitro-alert.nitro-alert-wanted .paradise-wanted-shell';
  const ALERT_SELECTOR = '.nitro-alert.nitro-alert-wanted';
  const INTERACTIVE_SELECTOR = 'button, input, textarea, select, a, [role="button"], [contenteditable="true"]';
  const DRAG_THRESHOLD = 4;
  const VIEWPORT_MARGIN = 6;

  let gesture = null;
  let suppressClickUntil = 0;

  const getShellFromEvent = event => event.target instanceof Element ? event.target.closest(SHELL_SELECTOR) : null;

  const finishDrag = pointerId => {
    if (!gesture || gesture.pointerId !== pointerId) return;

    const { shell, previousUserSelect, dragging } = gesture;

    try { shell.releasePointerCapture(pointerId); } catch (_) {}
    shell.classList.remove('is-dragging');
    document.body.style.userSelect = previousUserSelect;

    if (dragging) suppressClickUntil = performance.now() + 120;
    gesture = null;
  };

  document.addEventListener('pointerdown', event => {
    if (event.button !== 0) return;

    const shell = getShellFromEvent(event);
    if (!shell) return;

    // Controls must stay fully clickable. Dragging is available from every non-interactive surface.
    if (event.target.closest(INTERACTIVE_SELECTOR)) return;

    const alert = shell.closest(ALERT_SELECTOR);
    if (!alert) return;

    const rect = alert.getBoundingClientRect();
    const startTranslateX = Number.parseFloat(alert.dataset.paradiseDragX || '0') || 0;
    const startTranslateY = Number.parseFloat(alert.dataset.paradiseDragY || '0') || 0;

    gesture = {
      alert,
      shell,
      pointerId: event.pointerId,
      startX: event.clientX,
      startY: event.clientY,
      startRect: rect,
      startTranslateX,
      startTranslateY,
      previousUserSelect: document.body.style.userSelect,
      dragging: false
    };

    // Stop the legacy drag handler from converting the Nitro window to position:fixed.
    event.stopPropagation();

    try { shell.setPointerCapture(event.pointerId); } catch (_) {}
  }, true);

  document.addEventListener('pointermove', event => {
    if (!gesture || event.pointerId !== gesture.pointerId) return;

    const dx = event.clientX - gesture.startX;
    const dy = event.clientY - gesture.startY;

    if (!gesture.dragging) {
      if (Math.hypot(dx, dy) < DRAG_THRESHOLD) return;
      gesture.dragging = true;
      document.body.style.userSelect = 'none';
      gesture.shell.classList.add('is-dragging');
    }

    const { startRect } = gesture;
    const minDx = VIEWPORT_MARGIN - startRect.left;
    const maxDx = window.innerWidth - VIEWPORT_MARGIN - startRect.right;
    const minDy = VIEWPORT_MARGIN - startRect.top;
    const maxDy = window.innerHeight - VIEWPORT_MARGIN - startRect.bottom;

    const clampedDx = Math.min(maxDx, Math.max(minDx, dx));
    const clampedDy = Math.min(maxDy, Math.max(minDy, dy));

    const translateX = gesture.startTranslateX + clampedDx;
    const translateY = gesture.startTranslateY + clampedDy;

    gesture.alert.dataset.paradiseDragX = String(translateX);
    gesture.alert.dataset.paradiseDragY = String(translateY);
    gesture.alert.style.setProperty('translate', `${translateX}px ${translateY}px`, 'important');

    event.preventDefault();
    event.stopPropagation();
  }, true);

  document.addEventListener('pointerup', event => finishDrag(event.pointerId), true);
  document.addEventListener('pointercancel', event => finishDrag(event.pointerId), true);

  document.addEventListener('click', event => {
    if (performance.now() > suppressClickUntil) return;
    const shell = getShellFromEvent(event);
    if (!shell) return;

    // Never suppress a real control click. This keeps close, tabs and suspect cards reliable.
    if (event.target.closest(INTERACTIVE_SELECTOR)) return;

    event.preventDefault();
    event.stopImmediatePropagation();
  }, true);
})();
