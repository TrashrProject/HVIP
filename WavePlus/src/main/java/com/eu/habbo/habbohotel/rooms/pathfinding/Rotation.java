package com.eu.habbo.habbohotel.rooms.pathfinding;


public class Rotation {

	/**
	 * The directions to move in Left, right, up, down
	 */
	public static final short[][] DIRECTIONS = {{-1, 0}, {1, 0}, {0, -1}, {0, 1}};
	/**
	 * The diagonal directions to move in Top-right, bottom-left, top-left, bottom-right
	 */
	public static final short[][] DIAGONAL_DIRECTIONS = {{1, 1}, {-1, -1}, {-1, 1}, {1, -1}};

	private Rotation() {
		throw new IllegalStateException("Utility class");
	}

	public static int calculateRotation(int x1, int y1, int x2, int y2) {
		int dx = Integer.compare(x2, x1);
		int dy = Integer.compare(y2, y1);

		return getRotation(dx, dy);
	}

	private static Integer getRotation(int dx, int dy) {
		switch (dx) {
			case -1:
				Integer x = getNegativeRotation(dy);
				if (x != null) {
					return x;
				}
				break;
			case 0:
				Integer x3 = getZeroRotation(dy);
				if (x3 != null) {
					return x3;
				}
				break;
			case 1:
				Integer x4 = getPositiveRotation(dy);
				if (x4 != null) {
					return x4;
				}
				break;
			default:
				return 0;
		}
		return 0;
	}

	private static Integer getPositiveRotation(int dy) {
		switch (dy) {
			case -1:
				return 1;
			case 0:
				return 2;
			case 1:
				return 3;
			default:
				return null;
		}
	}

	private static Integer getZeroRotation(int dy) {
		switch (dy) {
			case -1:
				return 0;
			case 1:
				return 4;
			default:
				return null;
		}
	}

	private static Integer getNegativeRotation(int dy) {
		switch (dy) {
			case -1:
				return 7;
			case 0:
				return 6;
			case 1:
				return 5;
			default:
				return null;
		}
	}
}