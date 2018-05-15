import * as React from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import Layout from './Layout';
Ext.require('Ext.plugin.Responsive');
export default function App() {
    return (React.createElement(Router, null,
        React.createElement(Layout, null)));
}
//# sourceMappingURL=App.js.map