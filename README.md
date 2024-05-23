
## This repository contains the project files for the *Infrastructure Sensor Simulator* and the *Network Management Service*

## Overview

The Simulator Infrastructure System is a WPF MVVM application that represents a service (NetworkService) for monitoring measured values over entities in the system. The measurements are obtained from the MeteringSimulator application. NetworkService allows its users to view entities in the system in a tabular format, arrange them on a Drag&Drop grid, and visualize data using graphs. MeteringSimulator sends data to the NetworkService application at random intervals, indicating which entity the data is for.

## Target Audience

This application is developed for the following target group:

- **CG3 - Mobile Phone Users**: Users of mobile phones require a touch-oriented interface that is clear, and requires navigation that mobile users already know of. The interface should resemble a mobile phone emulator with a constant display of the navigation buttons, mimicking a real phone.

## Features

### Log Data Recording
- Each time a new measurement value is received, it is recorded in a log file (.txt) on the system disk. The log includes the timestamp, value, and reference to the corresponding entity.

### Network Entities View
- Users can add and delete entities for monitoring, with basic information displayed in a table format.
- Features include filtering and searching functionality for efficient data management.
- Filtering is done by the entity ID and/or its type. Based on the filter input, filter out the data and display it in the table.

### Network Display View
- Provides a space for visual representation of entities and their positions in the system/network.
- Users can arrange entities on a Drag&Drop grid and connect them with lines to represent relationships.
- Users can also remove items off the grid and place them back in a list

### Measurement Graph View
- Displays historical changes in measurements using graphs.
- Users can select entities to display on the graph and observe real-time updates.
- Different graph types are available for visualization.
- This specification implements an elliptical graph connected with a line, with entity values inside the ellipses. The ellipses change in color if the value is out of normal bounds.

### Entity Modeling
- Entities are modeled based on different types of measurements, each with specific attributes and valid value ranges.
- In this particular specification, an entity is modeled based on water flow meters.
- Users can interact with entities through the application interface, like adding, removing, placing them on a map and viewing entity analytic data.

## Contributors

- [Vladislav Petković](https://github.com/SaladVlad): Prototype, UX and UI Design, Back-End Implementation.

