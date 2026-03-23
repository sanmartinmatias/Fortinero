/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // RetroUI specific colors (Commonly used in their components)
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "#FF5733", // Example retro orange
          foreground: "#FFFFFF",
        },
      },
      borderRadius: {
        // Retro look usually uses sharp or very specific rounded corners
        none: '0',
        sm: '2px',
      },
      boxShadow: {
        // The "Neo-brutalist" shadow used in RetroUI
        retro: '4px 4px 0px 0px rgba(0,0,0,1)',
      }
    },
  },
 // plugins: [require("tailwindcss-animate")]
}