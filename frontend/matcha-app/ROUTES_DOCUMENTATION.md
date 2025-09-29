# Matcha App - Routes Documentation

This document describes all available routes and functionality in the Matcha Angular application.

## 🔐 Authentication Routes

### `/auth/login`
- **Component**: LoginComponent
- **Description**: User login page
- **Features**:
  - Username/password login form
  - Submit button to authenticate
  - Link to registration page
  - "Forgot password?" link
- **Navigation**: 
  - To registration: `/auth/register`
  - After login: redirects to `/users`

### `/auth/register`
- **Component**: RegistrationComponent
- **Description**: User registration page
- **Features**:
  - Registration form with fields:
    - Username
    - First Name
    - Last Name
    - Email
    - Password (with PrimeNG password component)
  - Submit button to create account
  - Back button to return to login
- **Navigation**:
  - Back to login: `/auth/login`

## 🏠 Main Application Routes (Protected by App Shell)

All routes below are wrapped in the `AppShellComponent` which includes:
- Navigation bar with menu items
- Home button
- Exit/logout functionality

### `/` (Root - Redirects to `/users`)
- **Redirect**: Automatically redirects to `/users`

### `/users`
- **Component**: UsersListComponent
- **Description**: Main users listing page
- **Features**: Display list of users
- **Navigation**: Available from navbar "Home" button

### `/users/:id`
- **Component**: UserProfileComponent
- **Description**: Individual user profile view
- **Features**: Display detailed user profile information
- **Parameters**: `:id` - User ID to display

### `/chat`
- **Component**: ChatComponent
- **Description**: Chat/messaging interface
- **Features**: Chat functionality
- **Navigation**: Available from navbar "Chat" menu item

### `/profile`
- **Component**: UserEditComponent
- **Description**: Edit current user's profile
- **Features**: Form to update user profile information
- **Navigation**: Available from navbar "Edit Profile" menu item

### `/settings`
- **Component**: UserSettingsComponent (with child routes)
- **Description**: User settings page with nested routes
- **Default**: Redirects to `/settings/password`
- **Navigation**: Available from navbar "Settings" menu item

#### `/settings/password`
- **Component**: PasswordEditComponent
- **Description**: Change password functionality
- **Features**: Form to update user password

#### `/settings/black-list`
- **Component**: BlackListComponent
- **Description**: Manage blocked users
- **Features**: View and manage user blacklist

## 🧭 Navigation Structure

### Navbar Menu Items
1. **Edit Profile** (`/profile`) - Edit user profile
2. **Chat** (`/chat`) - Access messaging
3. **Settings** (`/settings`) - User settings

### Navbar Actions
- **Home Button** - Navigate to users list (`/`)
- **Exit Button** - Logout functionality

## 🔄 Default Redirects

- `/` → `/users` (main app redirect)
- `/settings` → `/settings/password` (settings default)
- `/**` (any invalid route) → `/auth/login` (fallback)

## 📝 Notes

- All main application routes require the user to be within the app shell
- Authentication routes (`/auth/*`) are standalone without the app shell
- The application uses PrimeNG components for UI elements
- Styling is implemented with Tailwind CSS
- Forms use Angular Reactive Forms with form validation
