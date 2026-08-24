package io.github.brenoepics.roleplay.utilities.types;

import com.eu.habbo.habbohotel.users.HabboGender;
import lombok.Getter;

@Getter
public class Look {

  final HabboGender gender;
  final String lookString;

  public Look(HabboGender g, String s) {
    gender = g;
    lookString = s;
  }
}