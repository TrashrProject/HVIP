package com.eu.habbo.habbohotel.rooms.pathfinding;

import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import java.util.Map;

public class PathfinderContext {

	private RoomTile newTile;
	private RoomTile goalLocation;
	private RoomUnit roomUnit;
	private boolean isWalkthroughRetry;
	private boolean canMoveDiagonally;
	private RoomTile doorTile;
	private boolean allowWalkthrough;
	private Map<String, RoomTile> tileCache;

	public PathfinderContext(RoomTile newTile, RoomTile goalLocation, RoomUnit roomUnit,
			boolean isWalkthroughRetry, boolean canMoveDiagonally, RoomTile doorTile,
			boolean allowWalkthrough, Map<String, RoomTile> tileCache) {
		this.newTile = newTile;
		this.goalLocation = goalLocation;
		this.roomUnit = roomUnit;
		this.isWalkthroughRetry = isWalkthroughRetry;
		this.canMoveDiagonally = canMoveDiagonally;
		this.doorTile = doorTile;
		this.allowWalkthrough = allowWalkthrough;
		this.tileCache = tileCache;
	}

	public RoomTile getNewTile() {
		return this.newTile;
	}

	public RoomTile getGoalLocation() {
		return this.goalLocation;
	}

	public RoomUnit getRoomUnit() {
		return this.roomUnit;
	}

	public boolean isWalkthroughRetry() {
		return this.isWalkthroughRetry;
	}

	public boolean isCanMoveDiagonally() {
		return this.canMoveDiagonally;
	}

	public RoomTile getDoorTile() {
		return this.doorTile;
	}

	public boolean isAllowWalkthrough() {
		return this.allowWalkthrough;
	}

	public Map<String, RoomTile> getTileCache() {
		return this.tileCache;
	}

	public void setNewTile(RoomTile newTile) {
		this.newTile = newTile;
	}

	public void setGoalLocation(RoomTile goalLocation) {
		this.goalLocation = goalLocation;
	}

	public void setRoomUnit(RoomUnit roomUnit) {
		this.roomUnit = roomUnit;
	}

	public void setWalkthroughRetry(boolean isWalkthroughRetry) {
		this.isWalkthroughRetry = isWalkthroughRetry;
	}

	public void setCanMoveDiagonally(boolean canMoveDiagonally) {
		this.canMoveDiagonally = canMoveDiagonally;
	}

	public void setDoorTile(RoomTile doorTile) {
		this.doorTile = doorTile;
	}

	public void setAllowWalkthrough(boolean allowWalkthrough) {
		this.allowWalkthrough = allowWalkthrough;
	}

	public void setTileCache(Map<String, RoomTile> tileCache) {
		this.tileCache = tileCache;
	}
}