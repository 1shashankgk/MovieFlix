import React, { useEffect, useMemo, useState } from "react";
import axios from "axios";

interface Movie {
  imdbId: string;
  title: string;
  year: string;
  genre: string;
  poster: string;
  imdbRating?: string;
}

interface MovieDetails extends Movie {
  director?: string;
  actors?: string;
  plot?: string;
  runtime?: number;
  // add any other fields your backend returns
}

const ITEMS_PER_PAGE_DEFAULT = 8;
const API_BASE_URL = "https://movieflixwebapp-edg3dydaftgkbmgk.canadacentral-01.azurewebsites.net"

const MovieSearch: React.FC = () => {
  const [search, setSearch] = useState("");
  const [genre, setGenre] = useState("");
  const [year, setYear] = useState("");
  const [minRating, setMinRating] = useState("");

  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(false);
  const [noResults, setNoResults] = useState(false);

  // Sorting + pagination state
  const [sortBy, setSortBy] = useState<"rating" | "year" | "title">("rating");
  const [itemsPerPage, setItemsPerPage] = useState<number>(ITEMS_PER_PAGE_DEFAULT);
  const [currentPage, setCurrentPage] = useState<number>(1);

  // Modal state
  const [selectedMovieId, setSelectedMovieId] = useState<string | null>(null);
  const [movieDetails, setMovieDetails] = useState<MovieDetails | null>(null);
  const [modalLoading, setModalLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);

  // Search function (calls backend)
  const handleSearch = async () => {
    if (!search.trim()) return;
    setLoading(true);
    setNoResults(false);
    setMovies([]);
    setCurrentPage(1);

    try {
      const res = await axios.get(API_BASE_URL, {
        params: {
          search,
          genre: genre || undefined,
          year: year || undefined,
          minRating: minRating || undefined,
        },
      });

      const payload = res.data ?? [];
      if (!Array.isArray(payload) || payload.length === 0) {
        setNoResults(true);
        setMovies([]);
      } else {
        // Normalize keys (backend may supply lowercase or different props)
        const normalized: Movie[] = payload.map((m: any) => ({
          imdbId: m.imdbId ?? m.imdbID ?? m.imdbId ?? "",
          title: m.title ?? m.Title ?? "",
          year: m.year ?? m.Year ?? "",
          genre: Array.isArray(m.genre) ? m.genre.join(", ") : m.genre ?? "",
          poster: m.poster ?? m.Poster ?? "",
          imdbRating: m.imdbRating ?? m.imdbRating ?? m.imdbRating ?? (m.imdbRating ? String(m.imdbRating) : m.imdbRating) ?? m.imdbRating
        }));
        setMovies(normalized);
      }
    } catch (err) {
      console.error("Search error:", err);
      setNoResults(true);
      setMovies([]);
    } finally {
      setLoading(false);
    }
  };

  // Sorting: memoized list
  const sortedMovies = useMemo(() => {
    const arr = [...movies];
    switch (sortBy) {
      case "title":
        arr.sort((a, b) => a.title.localeCompare(b.title));
        break;
      case "year":
        arr.sort((a, b) => parseInt(b.year || "0") - parseInt(a.year || "0"));
        break;
      case "rating":
      default:
        arr.sort(
          (a, b) =>
            parseFloat(b.imdbRating ?? "0") - parseFloat(a.imdbRating ?? "0")
        );
        break;
    }
    return arr;
  }, [movies, sortBy]);

  // Pagination calculations
  const totalPages = Math.max(1, Math.ceil(sortedMovies.length / itemsPerPage));
  useEffect(() => {
    if (currentPage > totalPages) setCurrentPage(1);
  }, [totalPages, currentPage]);

  const paginatedMovies = useMemo(() => {
    const start = (currentPage - 1) * itemsPerPage;
    return sortedMovies.slice(start, start + itemsPerPage);
  }, [sortedMovies, currentPage, itemsPerPage]);

  // Modal: fetch details when selectedMovieId changes
  useEffect(() => {
    if (!selectedMovieId) return;

    let cancelled = false;
    const fetchDetails = async () => {
      setModalLoading(true);
      setMovieDetails(null);
      try {
        // Ensure backend endpoint exists: /api/movies/getById?imdbId=ttXXXX
        const res = await axios.get(
          `http://localhost:5045/api/movies/getById`,
          { params: { imdbId: selectedMovieId } }
        );
        if (!cancelled) {
          setMovieDetails(res.data ?? null);
          setModalOpen(true);
        }
      } catch (err) {
        console.error("Failed to fetch movie details:", err);
        if (!cancelled) {
          setMovieDetails(null);
          setModalOpen(true); // still open so user sees error message
        }
      } finally {
        if (!cancelled) setModalLoading(false);
      }
    };

    fetchDetails();
    return () => {
      cancelled = true;
    };
  }, [selectedMovieId]);

  // Close modal helper
  const closeModal = () => {
    setModalOpen(false);
    setSelectedMovieId(null);
    setMovieDetails(null);
  };

  // allow ESC to close modal
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") closeModal();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  return (
    <div className="flex flex-col items-center w-full">
      {/* controls row */}
      <div className="flex flex-wrap gap-3 w-full max-w-6xl items-center justify-between">
        <div className="flex flex-wrap gap-3 items-center">
          <input
            className="p-2 rounded-lg bg-gray-800 border border-gray-700 text-white w-64 focus:outline-none"
            placeholder="Search movies..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          />
          <input
            className="p-2 rounded-lg bg-gray-800 border border-gray-700 text-white w-40"
            placeholder="Genre"
            value={genre}
            onChange={(e) => setGenre(e.target.value)}
          />
          <input
            className="p-2 rounded-lg bg-gray-800 border border-gray-700 text-white w-28"
            placeholder="Year"
            value={year}
            onChange={(e) => setYear(e.target.value)}
          />
          <input
            className="p-2 rounded-lg bg-gray-800 border border-gray-700 text-white w-28"
            placeholder="Min rating"
            value={minRating}
            onChange={(e) => setMinRating(e.target.value)}
          />
          <button
            onClick={handleSearch}
            className="bg-blue-600 px-4 py-2 rounded-lg hover:bg-blue-700"
          >
            Search
          </button>
        </div>

        {/* sorting + pagination size */}
        <div className="flex gap-2 items-center">
          <label className="text-sm text-gray-400">Sort:</label>
          <select
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value as any)}
            className="bg-gray-800 border border-gray-700 p-2 rounded-lg"
          >
            <option value="rating">IMDb Rating</option>
            <option value="year">Year</option>
            <option value="title">Title</option>
          </select>

          <label className="text-sm text-gray-400 ml-3">Per page:</label>
          <select
            value={itemsPerPage}
            onChange={(e) => setItemsPerPage(Number(e.target.value))}
            className="bg-gray-800 border border-gray-700 p-2 rounded-lg"
          >
            <option value={4}>4</option>
            <option value={8}>8</option>
            <option value={12}>12</option>
            <option value={24}>24</option>
          </select>
        </div>
      </div>

      {/* status */}
      <div className="w-full max-w-6xl mt-6">
        {loading && (
          <div className="text-gray-400">🔍 Searching movies...</div>
        )}

        {!loading && noResults && (
          <div className="text-gray-400">🎞️ No results found.</div>
        )}
      </div>

      {/* grid */}
      {!loading && !noResults && paginatedMovies.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 mt-6 w-full max-w-6xl">
          {paginatedMovies.map((m) => (
            <div
              key={m.imdbId}
              className="bg-gray-800 rounded-lg overflow-hidden shadow hover:shadow-lg transition transform hover:scale-[1.02] cursor-pointer"
              onClick={() => setSelectedMovieId(m.imdbId)}
            >
              <div className="w-full h-72 bg-gray-700">
                <img
                  src={
                    m.poster && m.poster !== "N/A"
                      ? m.poster
                      : "https://via.placeholder.com/400x600?text=No+Poster"
                  }
                  alt={m.title}
                  className="w-full h-full object-cover"
                />
              </div>
              <div className="p-4">
                <h3 className="font-semibold text-lg line-clamp-2">{m.title}</h3>
                <p className="text-sm text-gray-400">{m.genre}</p>
                <div className="flex items-center justify-between mt-2">
                  <span className="text-sm text-gray-300">{m.year}</span>
                  <span className="text-yellow-400 font-semibold">
                    ⭐ {m.imdbRating ?? "—"}
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* pagination controls */}
      {!loading && !noResults && totalPages > 1 && (
        <div className="flex items-center gap-3 mt-6">
          <button
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            disabled={currentPage === 1}
            className="px-3 py-1 bg-gray-800 rounded disabled:opacity-50"
          >
            Prev
          </button>

          <div className="text-sm text-gray-300">
            Page {currentPage} of {totalPages}
          </div>

          <button
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            disabled={currentPage === totalPages}
            className="px-3 py-1 bg-gray-800 rounded disabled:opacity-50"
          >
            Next
          </button>
        </div>
      )}

      {/* Modal (overlay + content) */}
      {modalOpen && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center"
          aria-modal="true"
        >
          {/* overlay */}
          <div
            className="absolute inset-0 bg-black/60 backdrop-blur-sm"
            onClick={closeModal}
          />

          {/* modal card */}
          <div className="relative z-10 max-w-3xl w-full mx-4 bg-gray-900 rounded-xl shadow-lg overflow-hidden">
            <div className="flex justify-end p-3">
              <button
                onClick={closeModal}
                className="text-gray-300 hover:text-white rounded p-1"
                aria-label="Close"
              >
                ✕
              </button>
            </div>

            <div className="flex flex-col md:flex-row gap-4 p-6">
              {/* left: poster */}
              <div className="md:w-1/3 w-full h-80 bg-gray-700 rounded overflow-hidden">
                {modalLoading ? (
                  <div className="w-full h-full flex items-center justify-center text-gray-400">
                    Loading...
                  </div>
                ) : movieDetails ? (
                  <img
                    src={
                      movieDetails.poster && movieDetails.poster !== "N/A"
                        ? movieDetails.poster
                        : "https://via.placeholder.com/400x600?text=No+Poster"
                    }
                    alt={movieDetails.title}
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-gray-400">
                    No details available
                  </div>
                )}
              </div>

              {/* right: details */}
              <div className="md:w-2/3 w-full">
                {modalLoading ? (
                  <div className="text-gray-400">Fetching details...</div>
                ) : movieDetails ? (
                  <>
                    <h2 className="text-2xl font-bold mb-2">{movieDetails.title}</h2>
                    <div className="text-sm text-gray-400 mb-3">
                      {movieDetails.year} • {movieDetails.genre}
                    </div>
                    <p className="text-gray-300 mb-3">{movieDetails.plot}</p>

                    <div className="text-sm text-gray-400 space-y-1">
                      <div>
                        <span className="font-semibold text-gray-200">Director: </span>
                        {movieDetails.director ?? "N/A"}
                      </div>
                      <div>
                        <span className="font-semibold text-gray-200">Actors: </span>
                        {movieDetails.actors ?? "N/A"}
                      </div>
                      <div>
                        <span className="font-semibold text-gray-200">Runtime: </span>
                        {movieDetails.runtime ? `${movieDetails.runtime} mins` : "N/A"}
                      </div>
                      <div>
                        <span className="font-semibold text-gray-200">IMDb: </span>
                        {movieDetails.imdbRating ?? "N/A"}
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="text-gray-400">No details available</div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MovieSearch;
