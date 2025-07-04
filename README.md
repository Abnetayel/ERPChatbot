# ERP Chatbot Assistant

## Project Overview
A modern ERP Chatbot Assistant with a React/Vite frontend and ASP.NET Core backend. Designed for enterprise environments, it leverages Retrieval-Augmented Generation (RAG), context-based chat, ERP-topic restriction, and a professional chat UI.

## Key Features
- Floating chat widget with open/close controls
- ERP-topic restriction and context-aware responses
- Session-based chat history with recent chats panel
- Copy-to-clipboard for bot responses (with icon feedback)
- Configurable embedding source (HuggingFace API or local Python server)

## Technologies Used

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

## Installation & Setup

### Backend
```bash
cd ERPChatbotAssistant.Server
 dotnet restore
 dotnet run
```
Access Swagger UI: http://localhost:7001/swagger

### Frontend
```bash
cd erpchatbotassistant.client
npm install
npm run dev
```
Access the application: http://localhost:5173

## API Endpoints

### training data
-`http://localhost:5000/api/TrainingData/upload-csv`- upload training csv data

## Configuration
- Set the embedding endpoint in `Embeddings.cs` (backend) as needed.
- Set the openrouter model  endpoint in `ModelTrainingService.cs` (backend) as needed
- Customize UI/UX in `erpchatbotassistant.client/src/components/`.

## Usage
- Open [http://localhost:5173](http://localhost:5173) in your browser.
- Use the 💬 button to open chat, chevron to close.
- Access recent chats and session management via the menu.
- Copy bot responses with the copy icon.

## API Integration
- Frontend communicates with backend via REST API.
- Backend integrates with HuggingFace and Openrouter.

## Deployment
- Deploy frontend and backend separately (IIS, AWS, or on-premises).

## Contributing
1.Fork the repository
2.Create your feature branch
3.Commit your changes
4.Push to the branch
5.Create a new Pull Request

## Error Handling

The application includes comprehensive error handling:
- Network error detection
- Session initialization errors
- Message sending errors
- User-friendly error messages
- Retry options for failed operations