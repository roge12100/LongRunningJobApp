# Long Running Job Processor

A full-stack application demonstrating asynchronous job processing with real-time progress updates using WebSockets.

## Architecture

The application consists of three containerized services orchestrated via Docker Compose:
```
┌─────────────────────────────────────────────────────────────┐
│              Nginx API Gateway (Basic Auth)                 │
│                   (localhost:8080)                          │
└────────┬──────────────────────┬─────────────────────────────┘
         │                      │
         ▼                      ▼
┌─────────────────┐    ┌──────────────────────────────────────┐
│  Angular UI     │    │  .NET API + SignalR Hub              │
│  (port 3000)    │    │  (port 5000)                         │
│                 │    │                                      │
│  Served via     │    │  - REST API (/api/*)                 │
│  nginx proxy    │    │  - SignalR Hub (/hub/job-progress)   │
│                 │    │  - Background Job Processing         │
└─────────────────┘    └──────────────────────────────────────┘
```

**Key Features:**
- Reverse proxy architecture with nginx gateway as single entry point
- Basic authentication for secure access to all services
- Real-time job progress updates via SignalR WebSockets
- Character-by-character streaming of processed text
- Job cancellation support
- Clean separation between frontend, backend, and infrastructure layers

## Tech Stack

- **Frontend:** Angular 21 with Signals, PrimeNG UI components
- **Backend:** .NET 10 with ASP.NET Core, SignalR, Background Services
- **Gateway:** Nginx (reverse proxy, basic auth, ready to configure load balancing/rate-limiting)
- **Infrastructure:** Docker Compose

## Getting Started

### Prerequisites
- Docker & Docker Compose
- `htpasswd` utility (for generating auth credentials)
- (Optional) Node.js 20+ and .NET 10 SDK for local development

### Setup Authentication

Before running the application, create the password file:
```bash
# Generate .htpasswd file (replace with your credentials)
htpasswd -cb nginx/.htpasswd admin 'YourSecurePassword'
```

**Note:** The `.htpasswd` file is gitignored and should never be committed, as well as .env file.

### Running the Application

1. Clone the repository
2. Create authentication credentials (see above)
3. Run the application:
```bash
   docker-compose up --build
```
4. Access the application at **http://localhost:8080**
5. Login with the credentials you created

That's it! All services will start automatically with proper networking and health checks.

## Testing
This solution is covered by unit tests. 
- Backend (.NET API) unit tests can be run with: 
```bash
dotnet test ./backend 
```

- Frontend (Angular) unit tests can be run with:
```bash
cd frontend -> ng test
```

## Authentication

The nginx gateway implements HTTP Basic Authentication to protect all application routes:

- **Default credentials:** Set via `htpasswd` command (see Setup Authentication)
- **Protected routes:** All routes except `/health`
- **Login prompt:** Browser will display authentication dialog on first access

## Configuration

The only configurable external port is set in `.env`:
```env
API_EXTERNAL_PORT=8080  # External gateway port
ENVIRONMENT=Development
RESTART_POLICY=no
```

Internal containers ports (3000, 5000) are hardcoded for simplicity within the Docker network.

## Project Structure
```
.
├── backend/              # .NET API + SignalR Hub
├── frontend/             # Angular application
├── nginx/                # Nginx gateway configuration
│   ├── nginx.conf        # Reverse proxy & auth config
│   ├── Dockerfile
│   └── .htpasswd         # Auth credentials (gitignored)
├── docker-compose.yaml   # Service orchestration
├── .env                  # Environment configuration
└── README.md
```

## Usage

1. Access http://localhost:8080 and login
2. Enter text in the input field
3. Click "Start Job" to begin processing
4. Watch real-time character-by-character progress
5. Cancel job anytime if needed
6. View final results or start a new job

**Author:** Rogelio Castillo  
