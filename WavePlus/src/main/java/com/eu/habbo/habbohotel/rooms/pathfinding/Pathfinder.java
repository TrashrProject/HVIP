package com.eu.habbo.habbohotel.rooms.pathfinding;

import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import java.util.Deque;
import java.util.concurrent.CompletableFuture;

/**
 * The Pathfinder interface defines the contract for any class that will implement pathfinding
 * logic.
 */
public interface Pathfinder {

  /**
   * Asynchronously finds a path from the old tile to the new tile.
   *
   * @param oldTile      The starting tile.
   * @param newTile      The destination tile.
   * @param goalLocation The goal location tile.
   * @param roomUnit     The room unit for which the path is being found.
   * @return A deque of RoomTile objects representing the path from the old tile to the new tile.
   */
  CompletableFuture<Deque<RoomTile>> findPathAsync(RoomTile oldTile, RoomTile newTile,
      RoomTile goalLocation, RoomUnit roomUnit);

  /**
   * Finds a path from the old tile to the new tile.
   *
   * @param oldTile      The starting tile.
   * @param newTile      The destination tile.
   * @param goalLocation The goal location tile.
   * @param roomUnit     The room unit for which the path is being found.
   * @return A deque of RoomTile objects representing the path from the old tile to the new tile.
   */
  Deque<RoomTile> findPath(RoomTile oldTile, RoomTile newTile, RoomTile goalLocation,
      RoomUnit roomUnit);

  /**
   * Finds a path from the old tile to the new tile, with an option to retry if the first attempt
   * fails.
   *
   * @param oldTile            The starting tile.
   * @param newTile            The destination tile.
   * @param goalLocation       The goal location tile.
   * @param roomUnit           The room unit for which the path is being found.
   * @param isWalkthroughRetry If true, the method will retry finding a path if the first attempt
   *                           fails.
   * @return A deque of RoomTile objects representing the path from the old tile to the new tile.
   */
  Deque<RoomTile> findPath(RoomTile oldTile, RoomTile newTile, RoomTile goalLocation,
      RoomUnit roomUnit, boolean isWalkthroughRetry);

  /**
   * Simulates a step movement for a specific RoomUnit within a room.
   *
   * @param unit         The RoomUnit taking the step.
   * @param canFastWalk  Specifies whether fast walking is allowed for this step.
   * @return The resulting RoomTile after taking the step.
   */
  RoomTile takeStep(RoomUnit unit, boolean canFastWalk);

  /**
   * Finds an alternative walkable tile when the current path is blocked
   *
   * @param currentLocation The current location of the unit
   * @param blockedTile The tile that is blocked
   * @param nextTile The next tile in the original path
   * @param roomUnit The room unit needing an alternative path
   * @return An alternative walkable tile, or null if none found
   */
  RoomTile findAlternativePath(RoomTile currentLocation, RoomTile blockedTile, RoomTile nextTile, RoomUnit roomUnit);

  /**
   * Determines if the specified tile is walkable.
   *
   * @param tile The tile to check for walkability.
   * @return True if the tile is walkable, false otherwise.
   */
  boolean canWalkAt(RoomTile tile);

  /**
   * Determines whether it is possible to walk to the specified destination tile
   * from another tile, optionally checking for height compatibility.
   *
   * @param tile        The destination tile to check for walkability.
   * @param fromTile    The starting tile from which movement is attempted.
   * @param checkHeight A boolean flag indicating whether to check height compatibility
   *                    between the tiles.
   * @return True if the destination tile is walkable from the starting tile, false otherwise.
   */
  boolean canWalkAt(RoomTile tile, RoomTile fromTile, boolean checkHeight);

  /**
   * Checks if movement from one tile to another is walkable, including diagonal movement validation
   * and distance checks.
   *
   * @param from     The starting tile
   * @param to       The destination tile
   * @param roomUnit The room unit attempting the movement
   * @return True if the movement is walkable, false otherwise
   */
  boolean isWalkable(RoomTile from, RoomTile to, RoomUnit roomUnit);
}