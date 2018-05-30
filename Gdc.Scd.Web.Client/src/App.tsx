import * as React from 'react'
import { BrowserRouter as Router } from 'react-router-dom'
import { Provider, connect } from 'react-redux';
import { LayoutContainer } from './Layout/Components/Layout';
import { storeFactory } from './StoreFactory';

declare var Ext:any;

// Enable responsiveConfig app-wide. You can remove this if you don't plan to build a responsive UI.
Ext.require('Ext.plugin.Responsive');

const store = storeFactory();

/**
 * The main application view
 */
const App = () => {
    return (
        <Provider store={store}>
            <Router>
                <LayoutContainer/>
            </Router>
        </Provider>
    )
}

export default App;
