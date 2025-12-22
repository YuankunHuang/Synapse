import logging
import sys
import time
import math
import random
import threading
from dataclasses import dataclass
from signalrcore.hub_connection_builder import HubConnectionBuilder

handler = logging.StreamHandler()
handler.setLevel(logging.DEBUG)
server_url = "http://localhost:5241/gamehub"
botAreaSize = 500.0
botCount = 2000

@dataclass
class BotState:
    bot_id: str
    x: float = 0.0
    z: float = 0.0
    angle: float = 0.0
    speed: float = 0.0
    turn_speed: float = 0.0
    target_angle: float = 0.0

class BotManager:
    def __init__(self, bot_count: int):
        self.bots = []
        for i in range(bot_count):
            radius = random.random() * (botAreaSize / 2)
            theta = random.random() * 2 * math.pi
            
            self.bots.append(BotState(
                bot_id=f"python_bot_{i + 1}",
                x=radius * math.cos(theta),
                z=radius * math.sin(theta),
                angle=random.random() * 2 * math.pi,
                speed=random.uniform(3.0, 8.0),
                turn_speed=random.uniform(0.05, 0.2),
                target_angle=random.random() * 2 * math.pi
            ))
            
        self.conn = None
        self.connected_evt = threading.Event()

    def connect(self):
        self.conn = HubConnectionBuilder()\
            .with_url(server_url)\
            .configure_logging(logging.ERROR)\
            .with_automatic_reconnect({
                "type": "raw",
                "keep_alive_interval": 10,
                "reconnect_interval": 5,
                "max_attempts": 5,
            })\
            .build()

        self.conn.on_open(lambda: (print("[BotManager] Hub open"), self.connected_evt.set()))
        self.conn.on_close(lambda: (print("[BotManager] Hub closed"), self.connected_evt.clear()))
        self.conn.on_error(lambda err: (print(f"[BotManager] Hub error: {err}")))

        self.conn.start()

        if not self.connected_evt.wait(timeout=10):
            raise RuntimeError("SignalR handshake/open failed (timeout).")
        else:
            print("[BotManager] Connected")
    
    def tick(self):
        BOUNDARY_RADIUS = botAreaSize / 2
        dt = 0.1 

        for bot in self.bots:
            bot.target_angle += random.uniform(-0.5, 0.5)

            dist_sq = bot.x**2 + bot.z**2
            dist = math.sqrt(dist_sq)

            if dist > BOUNDARY_RADIUS:
                angle_to_center = math.atan2(-bot.z, -bot.x)
                diff = angle_to_center - bot.target_angle
                while diff > math.pi: diff -= 2 * math.pi
                while diff < -math.pi: diff += 2 * math.pi
                
                bot.target_angle += diff * 0.1

            diff = bot.target_angle - bot.angle
            while diff > math.pi: diff -= 2 * math.pi
            while diff < -math.pi: diff += 2 * math.pi
            
            bot.angle += diff * bot.turn_speed * dt * 5

            bot.x += math.cos(bot.angle) * bot.speed * dt
            bot.z += math.sin(bot.angle) * bot.speed * dt
        
    def sync(self):
        if not self.connected_evt.is_set():
            return

        all_bots = [{
            "Id": b.bot_id,
            "X": b.x,
            "Y": 0.0,
            "Z": b.z
        } for b in self.bots]

        # the entire payload tends to be too large to send
        # needs to be chunked
        current_chunk = []
        current_size = 0
        MAX_BYTES = 30000
        for bot in all_bots:
            item_size = len(str(bot)) + 2
            if current_size + item_size >= MAX_BYTES:
                # will exceed upper limit, send immediately & reset
                self.conn.send("SyncBotPosition", [current_chunk])
                current_chunk = []
                current_size = 0
            current_chunk.append(bot)
            current_size += item_size
        
        if current_chunk:
            self.conn.send("SyncBotPosition", [current_chunk])


    def run(self, tick_hz = 10, sync_hz = 10):
        tick_dt = 1.0 / tick_hz
        sync_dt = 1.0 / sync_hz
        
        start_time = time.time()
        next_tick_time = start_time
        next_sync_time = start_time

        print(f"[BotManager] Simulation started at {start_time}")

        while True:
            now = time.time()
            
            if now >= next_tick_time:
                self.tick()
                next_tick_time += tick_dt
            
            if now >= next_sync_time:
                self.sync()
                next_sync_time += sync_dt

            time.sleep(0.001)

if __name__ == "__main__":
    manager = BotManager(bot_count=botCount)
    manager.connect()
    manager.run()