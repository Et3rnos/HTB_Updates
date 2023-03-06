import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import AuthProvider from './contexts/AuthProvider';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import Customization from './pages/Customization';
import Home from './pages/Home';
import Callback from './pages/Callback';

const darkTheme = createTheme({
    palette: {
        mode: 'dark',
        primary: {
            light: '#757ce8',
            main: '#3f50b5',
            dark: '#002884',
            contrastText: '#fff',
        },
        secondary: {
            light: '#ff7961',
            main: '#f44336',
            dark: '#ba000d',
            contrastText: '#000',
        },
    },
    typography: {
        fontFamily: "'Ubuntu Mono', monospace"
    },
});

function App() {
    return (
        <ThemeProvider theme={darkTheme}>
            <CssBaseline />
            <AuthProvider>
                <BrowserRouter>
                    <Routes>
                        <Route path="/" element={<Home />} />
                        <Route path="/callback" element={<Callback />} />
                        <Route path="/customization" element={<Customization />} />
                    </Routes>
                </BrowserRouter>
            </AuthProvider>
        </ThemeProvider>
    );
}

export default App;
