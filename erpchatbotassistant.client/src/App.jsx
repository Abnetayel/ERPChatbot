import React, { useState } from 'react';
import ChatWindow from './components/ChatWindow';

function App() {
  const [messages, setMessages] = useState([]);

  // Simulate sending a message to the backend and getting a response
  const sendMessage = async (text) => {
    // Add user message
    const userMsg = { id: Date.now(), text, isUser: true };
    setMessages((msgs) => [...msgs, userMsg]);
    // Simulate bot response (replace with real API call)
    setTimeout(() => {
      setMessages((msgs) => [
        ...msgs,
        { id: Date.now() + 1, text: `Bot response to: ${text}`, isUser: false },
      ]);
    }, 500);
  };

  // // Handle editing a user message and re-sending it
  // const handleEditMessage = (id, newText) => {
  //   // Optionally update the message in state for display
  //   setMessages((msgs) =>
  //     msgs.map((msg) =>
  //       msg.id === id ? { ...msg, text: newText, edited: true } : msg
  //     )
  //   );
  //   // Re-send the edited message as a new message
  //   sendMessage(newText);
  // };

  return (
    <div className="min-h-screen bg-gray-100">
      <div className="container mx-auto px-4 py-8">
        <ChatWindow
          messages={messages}
          // onEditMessage={handleEditMessage}
        />
      </div>
    </div>
  );
}

export default App;