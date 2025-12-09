import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import App from '../App';

describe('App', () => {
  it('renders the main heading', () => {
    render(<App />);
    expect(screen.getByText('LiquorPOS Management System')).toBeDefined();
  });

  it('displays services status section', () => {
    render(<App />);
    expect(screen.getByText('Services Status')).toBeDefined();
  });
});
