"""Sample the GameCI container from the runner without reading its environment."""

from datetime import datetime, timezone
from pathlib import Path
import subprocess
import sys
import time


def docker(*args):
    try:
        return subprocess.run(
            ["docker", *args], check=True, text=True, timeout=4,
            stdout=subprocess.PIPE, stderr=subprocess.DEVNULL,
        ).stdout.strip()
    except (OSError, subprocess.SubprocessError):
        # The container can stop between discovery and sampling.
        return ""


def sample_cgroup(pid):
    if not pid.isdigit() or int(pid) <= 0:
        return
    try:
        memberships = Path(f"/proc/{pid}/cgroup").read_text().splitlines()
    except OSError:
        return

    group = next((line[3:] for line in memberships if line.startswith("0::/")), "")
    if not group or group == "/":
        return
    cgroup = Path("/sys/fs/cgroup") / group.lstrip("/")
    for name in ("memory.current", "memory.peak", "memory.max", "memory.events", "memory.swap.current"):
        try:
            value = (cgroup / name).read_text().strip().replace("\n", "; ")
        except OSError:
            continue
        print(f"{name} {value}")


def sample(container):
    print(f"\n{datetime.now(timezone.utc).isoformat()} container={container}")
    pid = docker("inspect", "--format", "{{.State.Pid}}", container)
    sample_cgroup(pid)
    # RSS is in KiB. comm is the executable name, not argv.
    processes = docker("top", container, "-eo", "pid,ppid,rss,comm")
    if processes:
        print(processes)
    stats = docker(
        "stats", "--no-stream", "--format",
        "cpu={{.CPUPerc}} memory={{.MemUsage}} pids={{.PIDs}}", container,
    )
    if stats:
        print(stats)


if __name__ == "__main__":
    image, output_directory = sys.argv[1:]
    output = Path(output_directory)
    print(f"{datetime.now(timezone.utc).isoformat()} Waiting for {image}")
    try:
        while not (output / ".stop").exists():
            for container in docker("ps", "-q", "--filter", f"ancestor={image}").splitlines():
                sample(container)
            time.sleep(1)
    finally:
        (output / ".stopped").touch()
