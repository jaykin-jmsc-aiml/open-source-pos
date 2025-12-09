import { useState, useEffect } from 'react'

interface ServiceStatus {
  service: string
  status: string
}

function App() {
  const [services, setServices] = useState<ServiceStatus[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchServices = async () => {
      try {
        const response = await fetch('http://localhost:5000/')
        const data = await response.json()
        setServices([data])
      } catch (error) {
        console.error('Failed to fetch services:', error)
      } finally {
        setLoading(false)
      }
    }

    fetchServices()
  }, [])

  return (
    <div className="min-h-screen bg-gray-100">
      <header className="bg-blue-600 text-white shadow-lg">
        <div className="container mx-auto px-4 py-6">
          <h1 className="text-3xl font-bold">LiquorPOS Management System</h1>
          <p className="text-blue-100 mt-2">Microservices Architecture</p>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-2xl font-semibold mb-4">Services Status</h2>
          
          {loading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
              <p className="mt-4 text-gray-600">Loading services...</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {[
                'Identity',
                'Catalog',
                'Inventory & Purchasing',
                'Sales & POS',
                'Customer Loyalty',
                'Reporting & Analytics',
                'Configuration'
              ].map((service) => (
                <div
                  key={service}
                  className="border border-gray-200 rounded-lg p-4 hover:shadow-lg transition-shadow"
                >
                  <div className="flex items-center justify-between">
                    <h3 className="font-semibold text-lg">{service}</h3>
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
                      Running
                    </span>
                  </div>
                  <p className="text-gray-600 text-sm mt-2">
                    Service is healthy and responding
                  </p>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="mt-8 bg-white rounded-lg shadow-md p-6">
          <h2 className="text-2xl font-semibold mb-4">Architecture Overview</h2>
          <div className="prose max-w-none">
            <p className="text-gray-700 mb-4">
              This system follows a microservices architecture with the following components:
            </p>
            <ul className="list-disc list-inside space-y-2 text-gray-700">
              <li><strong>Identity Service:</strong> User authentication and authorization</li>
              <li><strong>Catalog Service:</strong> Product catalog management</li>
              <li><strong>Inventory & Purchasing:</strong> Stock management and supplier orders</li>
              <li><strong>Sales & POS:</strong> Point of sale transactions</li>
              <li><strong>Customer Loyalty:</strong> Rewards and customer management</li>
              <li><strong>Reporting & Analytics:</strong> Business intelligence and insights</li>
              <li><strong>Configuration Service:</strong> System-wide settings</li>
            </ul>
          </div>
        </div>
      </main>

      <footer className="bg-gray-800 text-white mt-12">
        <div className="container mx-auto px-4 py-6 text-center">
          <p>&copy; 2024 LiquorPOS. All rights reserved.</p>
        </div>
      </footer>
    </div>
  )
}

export default App
