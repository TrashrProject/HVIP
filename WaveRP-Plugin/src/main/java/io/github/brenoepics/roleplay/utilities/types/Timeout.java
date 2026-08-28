package io.github.brenoepics.roleplay.utilities.types;

import java.time.Instant;
import lombok.Getter;

@Getter
public class Timeout
{
    private final int id;
    private final Instant finish;

    public Timeout(int id, Instant time) {
        this.id = id;
        this.finish = time;
    }

}
