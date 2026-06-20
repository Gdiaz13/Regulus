import { Moon, Sun } from 'lucide-react';
import { useEffect, useState } from 'react';
import { cn } from '../../lib/utils';

type ThemeName = 'dark' | 'light';

export const ThemeToggle = () => {
  const { isDarkMode, toggleTheme } = useThemeMode();
  return (
    <button type="button" onClick={toggleTheme} className={buttonClass()} aria-label={buttonLabel(isDarkMode)}>
      {themeIcon(isDarkMode)}
    </button>
  );
};

function useThemeMode() {
  const [isDarkMode, setIsDarkMode] = useState(false);
  useEffect(() => setTheme(readStoredTheme(), setIsDarkMode), []);
  const toggleTheme = () => setTheme(nextTheme(isDarkMode), setIsDarkMode);
  return { isDarkMode, toggleTheme };
}

function setTheme(theme: ThemeName, setIsDarkMode: (value: boolean) => void) {
  document.documentElement.classList.toggle('dark', theme === 'dark');
  localStorage.setItem('theme', theme);
  setIsDarkMode(theme === 'dark');
}

function readStoredTheme(): ThemeName {
  return localStorage.getItem('theme') === 'dark' ? 'dark' : 'light';
}

function nextTheme(isDarkMode: boolean): ThemeName {
  return isDarkMode ? 'light' : 'dark';
}

function buttonClass() {
  return cn('p-2 rounded-full transition-colors duration-300 focus:outline-hidden');
}

function buttonLabel(isDarkMode: boolean) {
  return isDarkMode ? 'Switch to light theme' : 'Switch to dark theme';
}

function themeIcon(isDarkMode: boolean) {
  return isDarkMode ? sunIcon() : moonIcon();
}

function sunIcon() {
  return <Sun className="h-6 w-6 text-yellow-300" />;
}

function moonIcon() {
  return <Moon className="h-6 w-6 text-blue-900" />;
}
