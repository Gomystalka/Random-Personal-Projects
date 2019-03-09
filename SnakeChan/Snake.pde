class Snake {
  private PVector head = new PVector(0, 0);
  private float segmentSize = 10;
  private int foodEaten = 0;

  private ArrayList<int[]> headOnionSkin = new ArrayList<int[]>();
  private float moveTimer = 0;
  private boolean desu = false, canMove = true;

  private Direction direction = Direction.EAST;

  public Snake() {
  }

  public void resetSnake() {
    foodEaten = 0;
  }

  public void update() {
    if (desu) return;
    if (moveTimer++ >= 60 / (timeScale <= 0 ? 1 : timeScale)) {
      canMove = true;
      moveTimer = 0;
      recordMovement(int(head.x), int(head.y));

      switch(direction) {
      case NORTH:
        head.y -= 1;
        break;
      case EAST:
        head.x += 1;
        break;
      case SOUTH:
        head.y += 1;
        break;
      case WEST:
        head.x -= 1;
        break;
      }
    }

    head.x = constrain(head.x, 0, cols - 1);
    head.y = constrain(head.y, 0, rows - 1);

    if (tiles[int(head.y)][int(head.x)] == TileState.FOOD) {
      tiles[int(head.y)][int(head.x)] = TileState.NONE;
      eat();
    }

    if (tiles[int(head.y)][int(head.x)] == TileState.SNAKE) { // This triggers when you eat yourself AND whenever you hit the bounds. Amazing, ain't it?
      desu = true;
    }

    if (canMove) {

      if (keys[KeyEvent.VK_UP] && direction != Direction.SOUTH) {
        direction = Direction.NORTH;
        canMove = false;
      }

      if (keys[KeyEvent.VK_DOWN] && direction != Direction.NORTH) {
        direction = Direction.SOUTH;
        canMove = false;
      }

      if (keys[KeyEvent.VK_LEFT] && direction != Direction.EAST) {
        direction = Direction.WEST;
        canMove = false;
      }

      if (keys[KeyEvent.VK_RIGHT] && direction != Direction.WEST) {
        direction = Direction.EAST;
        canMove = false;
      }
    }
  }

  public void render() {
    pushStyle();
    //stroke(255);
    //fill(headColor);
    //rect(head.x, head.y, segmentSize, segmentSize);
    tiles[(int)head.y][(int)head.x] = TileState.SNAKE_HEAD;

    for (int p = headOnionSkin.size() - 1; p >= 0; p--) {
      int x = headOnionSkin.get(p)[0]; //X
      int y = headOnionSkin.get(p)[1]; //Y
      tiles[y][x] = TileState.SNAKE;
    }      

    fill(75, 75, 255);
    textSize(36);
    textAlign(LEFT, CENTER);
    text((foodEaten * 100), 18, textAscent() - textDescent());
    popStyle();
  }

  public void eat() {
    foodEaten++;
    spawnFood();
    timeScale += 0.5f;
  }

  private void recordMovement(int x, int y) {
    int[] coords = {x, y};

    if (headOnionSkin.size() >= 4 + foodEaten) {
      int[] lastSegment = headOnionSkin.get(0);
      tiles[lastSegment[1]][lastSegment[0]] = TileState.NONE;
      headOnionSkin.remove(0);
    }

    headOnionSkin.add(coords);
  }

  public void setHeadPosition (PVector headPosition) {
    head = headPosition;
  }

  public float getSegmentsize() {
    return segmentSize;
  }

  public void setSegmentsize (float segmentSize) {
    this.segmentSize = segmentSize;
  }

  public int getFoodEaten() {
    return foodEaten;
  }

  public void setFoodEaten (int foodEaten) {
    this.foodEaten = foodEaten;
  }
}

enum Direction {
  NORTH, 
    EAST, 
    SOUTH, 
    WEST
}
