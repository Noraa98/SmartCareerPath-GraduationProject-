/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        background: "var(--background)",
        foreground: "var(--foreground)",
        card: "var(--card)",
        "card-foreground": "var(--card-foreground)",
        popover: "var(--popover)",
        "popover-foreground": "var(--popover-foreground)",
        primary: "var(--primary)",
        "primary-foreground": "var(--primary-foreground)",
        secondary: "var(--secondary)",
        "secondary-foreground": "var(--secondary-foreground)",
        muted: "var(--muted)",
        "muted-foreground": "var(--muted-foreground)",
        accent: "var(--accent)",
        "accent-foreground": "var(--accent-foreground)",
        destructive: "var(--destructive)",
        "destructive-foreground": "var(--destructive-foreground)",
        border: "var(--border)",
        input: "var(--input)",
        "input-background": "var(--input-background)",
        ring: "var(--ring)",
        // Custom AI Platform Colors - New Harmonious Palette
        dark: {
          DEFAULT: "#191D23",
          light: "#2a2f36",
        },
        slate: {
          DEFAULT: "#57707A",
          dark: "#455a64",
          light: "#6b8894",
        },
        "blue-gray": {
          DEFAULT: "#7E919F",
          dark: "#6a7d8b",
          light: "#92a5b3",
        },
        "gray-blue": {
          DEFAULT: "#979DAB",
          dark: "#7d8493",
          light: "#b1b6c4",
        },
        "purple-gray": {
          DEFAULT: "#C5BAC4",
          dark: "#b0a3b0",
          light: "#dad1d8",
        },
        cream: {
          DEFAULT: "#DEDCBC",
          dark: "#c9c7a7",
          light: "#e8e6d1",
        },
      },
      borderColor: {
        DEFAULT: "var(--border)",
      },
      outlineColor: {
        ring: "var(--ring)",
      },
    },
  },
  plugins: [],
}

