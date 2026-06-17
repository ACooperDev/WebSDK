import subprocess
import ipaddress
import platform
from concurrent.futures import ThreadPoolExecutor

class CogScanner:
    def __init__(self, timeout=50, max_workers=50):
        self.timeout = timeout
        self.max_workers = max_workers
        self.os = platform.system().lower()
        
    # Ping (cross-platform)
    def _ping(self, ip):
        subprocess.run(["ping", "-n" if self.os == "windows" else "-c", "1", "-w" if self.os == "windows" else "-W", str(self.timeout if self.os == "windows" else max(1, self.timeout // 1000)), ip], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    # Scan function
    def scan(self, network, mac_prefix=None):
        mac_prefix = mac_prefix.lower().replace("-", ":") if mac_prefix else None
        ips = map(str, ipaddress.ip_network(network).hosts())

        # Ping
        with ThreadPoolExecutor(self.max_workers) as ex:
            ex.map(self._ping, ips)
            
        # Select correct command, Windows vs other (Mac/Linux)
        cmd = ["arp", "-a"] if self.os == "windows" else ["ip", "neigh"]
        out = subprocess.run(cmd, capture_output=True, text=True).stdout.lower().splitlines()

        results = {}

        # Process the output for each OS
        for line in out:
            ip = mac = None

            if self.os == "windows" and "dynamic" in line:
                p = line.split()
                if len(p) >= 3:
                    ip, mac = p[0], p[1]

            elif self.os == "linux" and "lladdr" in line:
                p = line.split()
                try:
                    ip = p[0]
                    mac = p[p.index("lladdr") + 1]
                except ValueError:
                    continue

            elif self.os == "darwin" and " at " in line:
                try:
                    ip = line.split("(")[1].split(")")[0]
                    mac = line.split(" at ")[1].split()[0]
                except IndexError:
                    continue

            if not ip or not mac:
                continue

            mac = mac.replace("-", ":")
            if mac_prefix and not mac.startswith(mac_prefix):
                continue

            results[ip] = mac

        return results