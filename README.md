# ERP Chatbot Assistant

A modern chatbot assistant for ERP systems that helps users with various tasks through an interactive chat interface.

## Current Working Features

### Chat Interface
- Real-time chat window with message history
- User and bot message differentiation
- Message timestamps
- Typing indicators
- Error handling and retry options

### Bot Responses
- Time and date information
- Help menu with available features
- Inventory management assistance
- Sales and order tracking
- Customer information management
- Report generation options

### Technical Features
- Modern React frontend with Vite
- .NET Core backend API
- SQL Server database integration
- Swagger API documentation
- CORS enabled for development
- Error handling and logging

## Project Structure

```
ERPChatbotAssistant/
├── erpchatbotassistant.client/     # Frontend (React + Vite)
│   ├── src/
│   │   ├── components/            # React components
│   │   ├── services/             # API services
│   │   └── App.jsx              # Main application
│   └── package.json             # Frontend dependencies
│
└── ERPChatbotAssistant.Server/    # Backend (.NET Core)
    ├── Controllers/             # API endpoints
    ├── Services/               # Business logic
    ├── Models/                # Data models
    └── Data/                 # Database context
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 16+ and npm
- SQL Server

### Backend Setup
1. Navigate to the server directory:
   ```bash
   cd ERPChatbotAssistant.Server
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Access Swagger UI:
   ```
   http://localhost:7001/swagger
   ```

### Frontend Setup
1. Navigate to the client directory:
   ```bash
   cd erpchatbotassistant.client
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

4. Access the application:
   ```
   http://localhost:5173
   ```

## API Endpoints

### training data
-`http://localhost:5000/api/TrainingData/upload-csv`- upload training csv data

## Available Bot Commands

1. Help:
   - "help" - Shows all available features

2. Inventory:
   - "Check inventory levels"
   - "View inventory reports"
   - "Update inventory"

3. Sales:
   - "Create new order"
   - "Track order status"
   - "View sales history"

4. Customer:
   - "Search customer"
   - "View customer history"
   - "Update customer details"

5. Reports:
   - "Generate sales report"
   - "View inventory report"
   - "Get customer report"

## Development

### Frontend
- React with Vite for fast development
- Tailwind CSS for styling
- ESLint for code quality
- Hot Module Replacement (HMR)

### Backend
- .NET Core 8.0
- Entity Framework Core
- SQL Server database
- Swagger/OpenAPI documentation

## Error Handling

The application includes comprehensive error handling:
- Network error detection
- Session initialization errors
- Message sending errors
- User-friendly error messages
- Retry options for failed operations

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request