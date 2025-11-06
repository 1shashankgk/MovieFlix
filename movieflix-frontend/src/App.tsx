import MovieSearch from "./pages/MovieSearch";

function App() {
  return (
    <div className="min-h-screen w-screen bg-[#0b1120] text-white flex flex-col items-center justify-start px-4 py-10 overflow-x-hidden">
      {/* Header */}
      <header className="text-center mb-8">
        <div className="flex items-center justify-center gap-3 mb-2">
          <span className="text-5xl">🎬</span>
          <h1 className="text-4xl font-extrabold tracking-tight text-blue-400">
            MovieFlix
          </h1>
        </div>
        <p className="text-gray-400 text-sm sm:text-base">
          Discover movies by title, genre, year, or rating 🍿
        </p>
      </header>

      {/* Movie Search */}
      <main className="flex-grow w-full max-w-7xl">
        <MovieSearch />
      </main>
    </div>
  );
}

export default App;
