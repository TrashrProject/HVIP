package com.eu.habbo.plugin.events.rooms;

import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.plugin.Event;

public class HotelViewEvent extends Event {

  public final Habbo habbo;


  public HotelViewEvent(Habbo habbo) {
    this.habbo = habbo;
  }
}