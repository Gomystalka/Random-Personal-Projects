import java.awt.event.KeyEvent;

private Snake snakeChan;
public boolean[] keys = new boolean[1024];

public float timeScale = 4;

public TileState[][] tiles;
private int cellSize = 50;
private int rows, cols;

private float endY;

private PVector foodPos = new PVector();
private color foodHue;

void setup() {
  fullScreen();
  //size(500, 500);

  rows = floor(height / cellSize);
  cols = floor(width / cellSize);

  tiles = new TileState[rows][cols];

  for (int i = 0; i < rows; i++) {
    for (int k = 0; k < cols; k++) {
      tiles[i][k] = TileState.NONE;
    }
  }

  spawnFood();

  snakeChan = new Snake();
  snakeChan.setSegmentsize(cellSize);
  snakeChan.setHeadPosition(new PVector(rows / 2, cols / 2));
  
  endY = -(height * 1.25f);
}

void draw() {
  background(255);

  for (int r = 0; r < rows; r++) {
    for (int c = 0; c < cols; c++) {
      switch(tiles[r][c]) {
      case NONE:
        rect(c * cellSize, r * cellSize, cellSize, cellSize);
        break;
      case SNAKE:
        pushStyle();
        fill(0, 210, 0);
        rect(c * cellSize, r * cellSize, cellSize, cellSize);
        popStyle();
        break;
      case SNAKE_HEAD:
        pushStyle();
        fill(0, 180, 0);
        rect(c * cellSize, r * cellSize, cellSize, cellSize);
        fill(255);

        int centerX = c * cellSize + cellSize / 2;
        int centerY = r * cellSize + cellSize / 2;
        float eX = 0;
        float eY = 0;

        switch(snakeChan.direction) { //Add eyes to the snake =w= (I need to shorten this.)
        case NORTH:
          eX = map(foodPos.x, 0, width, (centerX + cellSize / 4) - cellSize / 10, (centerX + cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY - cellSize / 4) - cellSize / 10, (centerY - cellSize / 4) + cellSize / 10);
          ellipse(centerX + cellSize / 4, centerY - cellSize / 4, cellSize / 4, cellSize / 4);
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 1
          fill(255);
          ellipse(centerX - cellSize / 4, centerY - cellSize / 4, cellSize / 4, cellSize / 4);
          eX = map(foodPos.x, 0, width, (centerX - cellSize / 4) - cellSize / 10, (centerX - cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY - cellSize / 4) - cellSize / 10, (centerY - cellSize / 4) + cellSize / 10);
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 2

          //Mask//
          noFill();
          stroke(0, 180, 0);
          strokeWeight(4);
          ellipse(centerX - cellSize / 4, centerY - cellSize / 4, cellSize / 3, cellSize / 3);
          ellipse(centerX + cellSize / 4, centerY - cellSize / 4, cellSize / 3, cellSize / 3);
          break;
        case EAST:
          eX = map(foodPos.x, 0, width, (centerX + cellSize / 4) - cellSize / 10, (centerX + cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY - cellSize / 4) - cellSize / 10, (centerY - cellSize / 4) + cellSize / 10);
          ellipse(centerX + cellSize / 4, centerY - cellSize / 4, cellSize / 4, cellSize / 4); //Eye White 1
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 1
          fill(255);
          ellipse(centerX + cellSize / 4, centerY + cellSize / 4, cellSize / 4, cellSize / 4); //Eye White 2
          eX = map(foodPos.x, 0, width, (centerX + cellSize / 4) - cellSize / 10, (centerX + cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY + cellSize / 4) - cellSize / 10, (centerY + cellSize / 4) + cellSize / 10);
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 2

          //Mask//
          noFill();
          stroke(0, 180, 0);
          strokeWeight(4);
          ellipse(centerX + cellSize / 4, centerY - cellSize / 4, cellSize / 3, cellSize / 3);
          ellipse(centerX + cellSize / 4, centerY + cellSize / 4, cellSize / 3, cellSize / 3);
          break;
        case SOUTH:
          eX = map(foodPos.x, 0, width, (centerX + cellSize / 4) - cellSize / 10, (centerX + cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY + cellSize / 4) - cellSize / 10, (centerY + cellSize / 4) + cellSize / 10);
          ellipse(centerX + cellSize / 4, centerY + cellSize / 4, cellSize / 4, cellSize / 4); //Eye White 1
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 1
          fill(255);
          ellipse(centerX - cellSize / 4, centerY + cellSize / 4, cellSize / 4, cellSize / 4); //Eye White 2
          eX = map(foodPos.x, 0, width, (centerX - cellSize / 4) - cellSize / 10, (centerX - cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY + cellSize / 4) - cellSize / 10, (centerY + cellSize / 4) + cellSize / 10);
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 2

          //Mask//
          noFill();
          stroke(0, 180, 0);
          strokeWeight(4);
          ellipse(centerX + cellSize / 4, centerY + cellSize / 4, cellSize / 3, cellSize / 3);
          ellipse(centerX - cellSize / 4, centerY + cellSize / 4, cellSize / 3, cellSize / 3);
          break;
        case WEST:
          eX = map(foodPos.x, 0, width, (centerX - cellSize / 4) - cellSize / 10, (centerX - cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY - cellSize / 4) - cellSize / 10, (centerY - cellSize / 4) + cellSize / 10);
          ellipse(centerX - cellSize / 4, centerY - cellSize / 4, cellSize / 4, cellSize / 4); //Eye White 1
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 1
          fill(255);
          ellipse(centerX - cellSize / 4, centerY + cellSize / 4, cellSize / 4, cellSize / 4); //Eye White 2
          eX = map(foodPos.x, 0, width, (centerX - cellSize / 4) - cellSize / 10, (centerX - cellSize / 4) + cellSize / 10);
          eY = map(foodPos.y, 0, height, (centerY + cellSize / 4) - cellSize / 10, (centerY + cellSize / 4) + cellSize / 10);
          fill(0);
          ellipse(eX, eY, cellSize / 10, cellSize / 10); //Pupil 2

          //Mask//
          noFill();
          stroke(0, 180, 0);
          strokeWeight(4);
          ellipse(centerX - cellSize / 4, centerY - cellSize / 4, cellSize / 3, cellSize / 3);
          ellipse(centerX - cellSize / 4, centerY + cellSize / 4, cellSize / 3, cellSize / 3);
          break;
        }

        popStyle();
        break;
      case FOOD:
        pushStyle();
        colorMode(HSB);
        fill(foodHue, 255, 255);
        float foodSize = cellSize * abs(sin(radians(frameCount * 1.5f))) + cellSize * 1.5f;
        rect(c * cellSize, r * cellSize, cellSize, cellSize);
        popStyle();
        break;
      }
    }
  }
  snakeChan.update();
  snakeChan.render();

  if (snakeChan.desu) {
    endY = lerp(endY, 0, 0.05f);
    pushStyle();
    textSize(128);
    fill(75, 75, 255);
    textAlign(CENTER, CENTER);
    text("You lose!\nYour score: " + (snakeChan.foodEaten * 100), 0, endY, width, height);
    popStyle();
    fill(140, 0, 0);
  }
}

public void spawnFood() {
  int attempts = 0;
  while (attempts < (rows + cols)) {
    attempts++;
    int r = int(random(0, rows - 1));
    int c = int(random(0, cols - 1));
    if (tiles[r][c] != TileState.SNAKE) {
      tiles[r][c] = TileState.FOOD;
      foodPos = getNonGridPosition(r, c);
      foodHue = int(random(0, 255));
      break;
    }
  }

  if (attempts >= (rows + cols)) {
    println("No spawn locations found for food! How?!");
  }
}

private PVector getNonGridPosition(float x, float y) {
  x = x * cellSize;
  y = y * cellSize;
  return new PVector(y, x);
}

void keyPressed() {
  keys[keyCode] = true;
}

void keyReleased() {
  keys[keyCode] = false;
}

//Not required but I'm lazy af.
public static String getGettersAndSettersOf(String field, String classChan) {
  String old = field;
  String firstChar = field.substring(0, 1).toUpperCase();
  field = firstChar + field.substring(1);
  String setter = "public void set" + field + " (" + classChan + " " + old + ") {" + "\n\tthis." + old + " = " + old + ";\n}";
  String getter = "public " + classChan + " get" + field + "() {\n\treturn " + old + ";\n}";
  return getter + "\n\n" + setter;
}

enum TileState {
  NONE, 
    SNAKE, 
    SNAKE_HEAD, 
    FOOD
}
