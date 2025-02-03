# Woodwork Management App

WPF desktop application designed for woodworking businesses to manage products, calculate volumes, and handle pricing for various wood types. Built using C# and the MVVM architectural pattern.

## Features

### Product Management
- Comprehensive product catalog management  
- Import/Export functionality with Excel integration  
- Product reordering  
- Quick search capabilities  
- Product duplication and editing  

### Volume Calculator
- Unit conversion (millimeters, centimeters, decimeters to meters)  
- Cuboid volume calculation with dimensions input  
- Cylinder volume calculation  
- Real-time volume updates  
- Direct product selection

### Shopping Cart
- Dynamic cart management  
- Add/remove products  
- Volume-based pricing  
- Collapsible cart panel   

### User Interface
- Modern, clean design  
- Custom window chrome  
- Responsive layout  
- Dark theme with accent colors  
- Intuitive navigation  

## Technical Details

### Built With
- C#  
- WPF (.NET Core)  
- MVVM Architecture  
- Microsoft.Extensions.DependencyInjection  
- EPPlus for Excel operations  
- Custom JSON storage system  

### Key Components
- Custom controls and styles  
- XAML-based UI  
- Asynchronous data loading  
- File system integration  
- Service-based architecture  

### Design Patterns
- MVVM (Model-View-ViewModel)  
- Dependency Injection  
- Repository Pattern  
- Command Pattern  
- Service Locator  

## Configuration
The application stores its data in:
```
%AppData%\WoodworkManagementApp
```

## Future Development
In near future planning to add: Products Categories, Price Page with functionalities of creating offers for clients, Order Page with fast generating of order based on calculated products.
