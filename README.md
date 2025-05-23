# LoadBalancer.DistributedAlgorithms2

Welcome to the **LoadBalancer.DistributedAlgorithms2** project! This is a fun and educational distributed system built with .NET 9, designed to explore load balancing, fault tolerance, and monitoring.

> 🗓️ This project distributes computational tasks across worker nodes, caches results with Redis, and monitors metrics with Prometheus and Grafana — all within Docker containers.

---

## 🚀 Overview

This project simulates a distributed system where a load balancer distributes calculation requests to worker nodes. It features:

- A single .NET 9 codebase that acts as either a load balancer or worker node.
- Redis caching to boost performance.
- Prometheus + Grafana for monitoring.
- Fault-tolerant load balancing.
- Fully containerized using Docker.

---

## ✨ Features

- **Load Balancing**: Distributes tasks to 3 worker instances.
- **Fault Tolerance**: Uses Polly for retries and circuit breakers.
- **Caching**: Caches results in Redis to prevent redundant work.
- **Monitoring**: Collects and visualizes metrics.
- **Dockerized**: Runs seamlessly in containers.

---

## 🛠 Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (with WSL2 enabled on Windows)
- .NET 9 SDK *(optional for local dev)*
- PowerShell / Command Prompt

### Installation

1. **Clone the Repository**:
    ```bash
    git clone https://github.com/mohammedabdelaleem/LoadBalancer.DistributedAlgorithms2.git
    cd LoadBalancer.DistributedAlgorithms2
    ```

2. **Build and Run**:
    ```bash
    docker-compose down --rmi all
    docker system prune -a -f
    docker-compose up --build
    ```
    > The first build may take several minutes.

3. **Verify**:
    - Look for Redis and app logs in the terminal.
    - If it fails, check the Troubleshooting section below.

---

## ⚙️ Usage

### Run a Calculation

```bash
curl "http://localhost:3000/api/calculation/cal?n=1"
```

## Check Worker Health

```bash
curl http://localhost:5002/health
```


## View Metrics

```bash
curl http://localhost:3000/metrics
```

## Simulate a Worker Failure

```bash
docker-compose stop worker2
```



## Stop the System

```bash
docker-compose down
```


