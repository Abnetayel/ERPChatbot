import React from 'react';
import ChatWindow from './components/ChatWindow';

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <div className="container mx-auto px-4 py-8">
        <ChatWindow />
      </div>
    </div>
  );
}

export default App;