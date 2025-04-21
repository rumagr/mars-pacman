import copy
import pygame
import math
import threading
import websockets
import json
import asyncio

pygame.init()
pygame.display.set_caption("MARS PacMan")
icon = pygame.image.load('assets/player_images/1.png')
pygame.display.set_icon(icon)


boards = [
[6, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5],
[3, 6, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 6, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 3],
[3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3],
[3, 3, 1, 6, 4, 4, 5, 1, 6, 4, 4, 4, 5, 1, 3, 3, 1, 6, 4, 4, 4, 5, 1, 6, 4, 4, 5, 1, 3, 3],
[3, 3, 2, 3, 0, 0, 3, 1, 3, 0, 0, 0, 3, 1, 3, 3, 1, 3, 0, 0, 0, 3, 1, 3, 0, 0, 3, 2, 3, 3],
[3, 3, 1, 7, 4, 4, 8, 1, 7, 4, 4, 4, 8, 1, 7, 8, 1, 7, 4, 4, 4, 8, 1, 7, 4, 4, 8, 1, 3, 3],
[3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3],
[3, 3, 1, 6, 4, 4, 5, 1, 6, 5, 1, 6, 4, 4, 4, 4, 4, 4, 5, 1, 6, 5, 1, 6, 4, 4, 5, 1, 3, 3],
[3, 3, 1, 7, 4, 4, 8, 1, 3, 3, 1, 7, 4, 4, 5, 6, 4, 4, 8, 1, 3, 3, 1, 7, 4, 4, 8, 1, 3, 3],
[3, 3, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 3, 3],
[3, 7, 4, 4, 4, 4, 5, 1, 3, 7, 4, 4, 5, 0, 3, 3, 0, 6, 4, 4, 8, 3, 1, 6, 4, 4, 4, 4, 8, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 3, 6, 4, 4, 8, 0, 7, 8, 0, 7, 4, 4, 5, 3, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 3, 3, 0, 6, 4, 4, 9, 9, 4, 4, 5, 0, 3, 3, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 7, 8, 0, 3, 0, 0, 0, 0, 0, 0, 3, 0, 7, 8, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 6, 5, 0, 3, 0, 0, 0, 0, 0, 0, 3, 0, 6, 5, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 3, 3, 0, 7, 4, 4, 4, 4, 4, 4, 8, 0, 3, 3, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 0, 0, 0, 0, 0, 3, 1, 3, 3, 0, 6, 4, 4, 4, 4, 4, 4, 5, 0, 3, 3, 1, 3, 0, 0, 0, 0, 0, 3],
[3, 6, 4, 4, 4, 4, 8, 1, 7, 8, 0, 7, 4, 4, 5, 6, 4, 4, 8, 0, 7, 8, 1, 7, 4, 4, 4, 4, 5, 3],
[3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3],
[3, 3, 1, 6, 4, 4, 5, 1, 6, 4, 4, 4, 5, 1, 3, 3, 1, 6, 4, 4, 4, 5, 1, 6, 4, 4, 5, 1, 3, 3],
[3, 3, 1, 7, 4, 5, 3, 1, 7, 4, 4, 4, 8, 1, 7, 8, 1, 7, 4, 4, 4, 8, 1, 3, 6, 4, 8, 1, 3, 3],
[3, 3, 2, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 2, 3, 3],
[3, 7, 4, 5, 1, 3, 3, 1, 6, 5, 1, 6, 4, 4, 4, 4, 4, 4, 5, 1, 6, 5, 1, 3, 3, 1, 6, 4, 8, 3],
[3, 6, 4, 8, 1, 7, 8, 1, 3, 3, 1, 7, 4, 4, 5, 6, 4, 4, 8, 1, 3, 3, 1, 7, 8, 1, 7, 4, 5, 3],
[3, 3, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 3, 3],
[3, 3, 1, 6, 4, 4, 4, 4, 8, 7, 4, 4, 5, 1, 3, 3, 1, 6, 4, 4, 8, 7, 4, 4, 4, 4, 5, 1, 3, 3],
[3, 3, 1, 7, 4, 4, 4, 4, 4, 4, 4, 4, 8, 1, 7, 8, 1, 7, 4, 4, 4, 4, 4, 4, 4, 4, 8, 1, 3, 3],
[3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3],
[3, 7, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 8, 3],
[7, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 8]
         ]

WIDTH = 900
HEIGHT = 950
TILE_WIDTH = WIDTH // 30      
TILE_HEIGHT = (HEIGHT - 50) // 32  
X_OFFSET = -5
Y_OFFSET = -10
URI = "ws://127.0.0.1:8181"
DATA_PROCESSED_EVENT = threading.Event()
CURRENT_TICK = 1

screen = pygame.display.set_mode([WIDTH, HEIGHT])
timer = pygame.time.Clock()
fps = 60
font = pygame.font.Font('freesansbold.ttf', 20)
level = copy.deepcopy(boards)
color = 'blue'
PI = math.pi
player_images = []
for i in range(1, 5):
    player_images.append(pygame.transform.scale(pygame.image.load(f'assets/player_images/{i}.png'), (45, 45)))
blinky_img = pygame.transform.scale(pygame.image.load(f'assets/ghost_images/red.png'), (45, 45))
pinky_img = pygame.transform.scale(pygame.image.load(f'assets/ghost_images/pink.png'), (45, 45))
inky_img = pygame.transform.scale(pygame.image.load(f'assets/ghost_images/blue.png'), (45, 45))
clyde_img = pygame.transform.scale(pygame.image.load(f'assets/ghost_images/orange.png'), (45, 45))
spooked_img = pygame.transform.scale(pygame.image.load(f'assets/ghost_images/powerup.png'), (45, 45))
dead_img = pygame.transform.scale(pygame.image.load(f'assets/ghost_images/dead.png'), (45, 45))
player_x = 450
player_y = 663
direction = 0
blinky_x = 440
blinky_y = 388
inky_x = 440
inky_y = 388
pinky_x = 440
pinky_y = 438
clyde_x = 440
clyde_y = 438
blinky_mode = "Chase"
inky_mode = "Chase"
pinky_mode = "Chase"
clyde_mode = "Chase"
counter = 0
flicker = False
score = 0
powerup = False
power_counter = 0
eaten_ghost = [False, False, False, False]
targets = [(player_x, player_y), (player_x, player_y), (player_x, player_y), (player_x, player_y)]
blinky_dead = False
inky_dead = False
clyde_dead = False
pinky_dead = False
lives = 3
game_over = False
game_won = False

def draw_misc():
    score_text = font.render(f'Score: {score}', True, 'white')
    screen.blit(score_text, (10, 920))
    if powerup:
        pygame.draw.circle(screen, 'blue', (140, 930), 15)
    for i in range(lives):
        screen.blit(pygame.transform.scale(player_images[0], (30, 30)), (650 + i * 40, 915))

def draw_board():
    for i in range(len(level)):
        for j in range(len(level[i])):
            if level[i][j] == 1:
                pygame.draw.circle(screen, 'white', (j * TILE_WIDTH + (0.5 * TILE_WIDTH), i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)), 4)
            if level[i][j] == 2 and not flicker:
                pygame.draw.circle(screen, 'white', (j * TILE_WIDTH + (0.5 * TILE_WIDTH), i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)), 10)
            if level[i][j] == 3:
                pygame.draw.line(screen, color, (j * TILE_WIDTH + (0.5 * TILE_WIDTH), i * TILE_HEIGHT),
                                 (j * TILE_WIDTH + (0.5 * TILE_WIDTH), i * TILE_HEIGHT + TILE_HEIGHT), 3)
            if level[i][j] == 4:
                pygame.draw.line(screen, color, (j * TILE_WIDTH, i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)),
                                 (j * TILE_WIDTH + TILE_WIDTH, i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)), 3)
            if level[i][j] == 5:
                pygame.draw.arc(screen, color, [(j * TILE_WIDTH - (TILE_WIDTH * 0.4)) - 2, (i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)), TILE_WIDTH, TILE_HEIGHT],
                                0, PI / 2, 3)
            if level[i][j] == 6:
                pygame.draw.arc(screen, color,
                                [(j * TILE_WIDTH + (TILE_WIDTH * 0.5)), (i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)), TILE_WIDTH, TILE_HEIGHT], PI / 2, PI, 3)
            if level[i][j] == 7:
                pygame.draw.arc(screen, color, [(j * TILE_WIDTH + (TILE_WIDTH * 0.5)), (i * TILE_HEIGHT - (0.4 * TILE_HEIGHT)), TILE_WIDTH, TILE_HEIGHT], PI,
                                3 * PI / 2, 3)
            if level[i][j] == 8:
                pygame.draw.arc(screen, color,
                                [(j * TILE_WIDTH - (TILE_WIDTH * 0.4)) - 2, (i * TILE_HEIGHT - (0.4 * TILE_HEIGHT)), TILE_WIDTH, TILE_HEIGHT], 3 * PI / 2,
                                2 * PI, 3)
            if level[i][j] == 9:
                pygame.draw.line(screen, 'white', (j * TILE_WIDTH, i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)),
                                 (j * TILE_WIDTH + TILE_WIDTH, i * TILE_HEIGHT + (0.5 * TILE_HEIGHT)), 3)


def draw_player():
    if direction == 0:
        screen.blit(player_images[counter // 5], (player_x, player_y))
    elif direction == 1:
        screen.blit(pygame.transform.flip(player_images[counter // 5], True, False), (player_x, player_y))
    elif direction == 2:
        screen.blit(pygame.transform.rotate(player_images[counter // 5], 90), (player_x, player_y))
    elif direction == 3:
        screen.blit(pygame.transform.rotate(player_images[counter // 5], 270), (player_x, player_y))

def draw_ghost(idx, x, y, mode):
    if mode == "Eaten":
        screen.blit(dead_img, (x, y))
    elif mode == "Frightened":
        screen.blit(spooked_img, (x, y))
    else:
        if idx == 0:
            screen.blit(blinky_img, (x, y))
        elif idx == 1:
            screen.blit(inky_img, (x, y))
        elif idx == 2:
            screen.blit(pinky_img, (x, y))
        elif idx == 3:
            screen.blit(clyde_img, (x, y))


def apply_game_state(data):
    global player_x, player_y, direction, score, lives, powerup
    global blinky_x, blinky_y, pinky_x, pinky_y, inky_x, inky_y, clyde_x, clyde_y
    global blinky_mode, inky_mode, pinky_mode, clyde_mode
    global level

    map_height = len(level) - 1

    score = data["score"]

    for agent in data["agents"]:
        y = map_height - agent["y"]  

        if agent["type"] == "PacManAgent":
            player_x = agent["x"] * TILE_WIDTH + X_OFFSET
            player_y = y * TILE_HEIGHT + Y_OFFSET
            lives = agent["lives"]
            powerup = agent["poweredUp"]
            direction = {"Right": 0, "Left": 1, "Up": 2, "Down": 3}.get(agent["direction"], 0)

        elif agent["type"] == "GhostAgent":
            gx = agent["x"] * TILE_WIDTH + X_OFFSET
            gy = y * TILE_HEIGHT + Y_OFFSET
            mode = agent["mode"]

            if agent["name"] == "Blinky":
                blinky_x, blinky_y, blinky_mode = gx, gy, mode
            elif agent["name"] == "Pinky":
                pinky_x, pinky_y, pinky_mode = gx, gy, mode
            elif agent["name"] == "Inky":
                inky_x, inky_y, inky_mode = gx, gy, mode
            elif agent["name"] == "Clyde":
                clyde_x, clyde_y, clyde_mode = gx, gy, mode

    for i in range(len(level)):
        for j in range(len(level[i])):
            if level[i][j] in [1, 2]:
                level[i][j] = 0

    for agent in data["agents"]:
        if agent["type"] == "Pellet":
            level[map_height - agent["y"]][agent["x"]] = 1
        elif agent["type"] == "PowerPellet":
            level[map_height - agent["y"]][agent["x"]] = 2

async def websocket_loop():
    global CURRENT_TICK, AGENTS

    async with websockets.connect(URI) as ws:
        await ws.send(str(CURRENT_TICK))

        while True:
            message = await ws.recv()

            if message.strip().lower() == "close":
                        pygame.event.post(pygame.event.Event(pygame.QUIT))
                        break

            agent_data = json.loads(message)
            apply_game_state(agent_data)

            DATA_PROCESSED_EVENT.wait()
            DATA_PROCESSED_EVENT.clear()

            CURRENT_TICK += 1
            await ws.send(str(CURRENT_TICK))

def start_ws_thread():
    asyncio.run(websocket_loop())

if __name__ == "__main__":
    threading.Thread(target=start_ws_thread, daemon=True).start()
    run = True
    while run:
        timer.tick(fps)
        timer.tick(fps)
        if counter < 19:
            counter += 1
            if counter > 3:
                flicker = False
        else:
            counter = 0
            flicker = True

        screen.fill('black')
        draw_board()
        draw_player()
        draw_ghost(0, blinky_x, blinky_y, blinky_mode)
        draw_ghost(1, inky_x, inky_y, inky_mode)
        draw_ghost(2, pinky_x, pinky_y, pinky_mode)
        draw_ghost(3, clyde_x, clyde_y, clyde_mode)
        
        draw_misc()


        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                run = False

        pygame.display.flip()
        DATA_PROCESSED_EVENT.set()
    pygame.quit()

