# Woodwork Management App

A WPF desktop application designed for woodworking businesses to manage products, calculate volumes, and handle pricing for various wood types. Built using C# and the MVVM architectural pattern.

## Features

### Product Management
- Comprehensive product catalog management with name, price per m³, and discount thresholds
- Import/Export functionality with Excel integration using EPPlus
- Product reordering with drag-and-drop capability
- Quick search capabilities with real-time filtering
- Product duplication and editing functionality
- Product pricing with volume-based discount system

### Volume Calculator
- Unit conversion (millimeters, centimeters, decimeters to meters)
- Cuboid volume calculation with width, height, and length inputs
- Cylinder volume calculation with radius and length inputs
- Real-time volume updates with quantity support
- Direct product selection with multi-select capability
- Shopping cart integration for calculated volumes

### Price Management
- Dynamic price calculations based on volume and quantity
- Discount system with volume thresholds
- Real-time price updates
- Per-piece price calculation
- Total price summaries with discounts
- Volume and piece quantity tracking
- Order generation capability

### Shopping Cart
- Dynamic cart management with live updates
- Add/remove products with quantities
- Volume-based pricing with automatic calculations
- Collapsible cart panel with persistent storage
- Direct transfer to price page for detailed calculations

### User Interface
- Modern, clean design with consistent styling
- Custom window chrome for a native look
- Responsive layout with dynamic resizing
- Dark theme with green and brown accent colors
- Intuitive navigation between calculator and price pages
- Custom styled buttons, input boxes, and scrollbars

## Technical Details

### Built With
- C# / WPF (.NET Core)
- MVVM Architecture
- `Microsoft.Extensions.DependencyInjection` for dependency injection
- `EPPlus` for Excel operations
- Custom JSON storage system for data persistence

### Key Components
- Custom XAML controls and styles
- Asynchronous data operations
- Real-time calculation updates
- File system integration for data storage
- Service-based architecture with interfaces

## Architecture

### Services
- `IProductService` - Product management and persistence
- `IPriceService` - Price calculations and item management
- `ICartService` - Shopping cart operations
- `IJsonStorageService` - Data persistence

### ViewModels
- `CalcPageViewModel` - Volume calculations and cart operations
- `PricePageViewModel` - Price management and calculations
- `ProductsViewModel` - Product catalog management

### Models
- `Product` - Product information and pricing
- `PriceItem` - Price calculations and discounts
- `CartItem` - Shopping cart items

### Design Patterns
- MVVM (Model-View-ViewModel) with proper data binding
- Dependency Injection using `Microsoft.Extensions.DependencyInjection`
- Repository Pattern for data access
- Command Pattern for UI actions
- Observer Pattern for property change notifications

## Data Storage
The application stores its data in JSON format:
```
%AppData%\WoodworkManagementApp\
  ├── products.json    # Product catalog
  ├── cart.json       # Shopping cart state
  └── price_items.json # Price calculations
```

## Features in Development
- Order generation system
- Enhanced product categorization