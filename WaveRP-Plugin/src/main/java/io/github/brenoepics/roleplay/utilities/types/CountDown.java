package io.github.brenoepics.roleplay.utilities.types;

import java.time.Instant;
import java.util.HashMap;

/**
 * A class that implements timeout behavior for acts.
 *
 * @author BrenoEpic
 */
public class CountDown {

    /**
     * A hashmap to store the action timeout information, where the key is the user id and the value is a Timeout object.
     */
    private final HashMap<Integer, Timeout> users;

    /**
     * Creates a new instance of CountDown
     */
    public CountDown() {
        this.users = new HashMap<>();
    }

    /**
     * Clears all timeout data.
     */
    public void dispose() {
        this.users.clear();
    }

    /**
     * Checks if a user is able to act.
     *
     * @param user The id of the user to be checked.
     * @return Returns the Timeout object if the user is not able to act, otherwise returns null.
     */
    public Timeout getTimeOut(int user) {
        if (this.users.containsKey(user)) {
            Timeout timeout = this.users.get(user);
            if (timeout.getFinish().isAfter(Instant.now())) {
                return timeout;
            }
            this.users.remove(user);
        }
        return null;
    }

    /**
     * Adds a new timeout for a user.
     *
     * @param user The id of the user.
     * @param time The duration of the timeout in seconds.
     */
    public void addTimeOut(int user, int time) {
        this.users.put(user, new Timeout(user, Instant.now().plusSeconds(time)));
    }
}
