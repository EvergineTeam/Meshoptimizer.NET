# MeshOptimizer.NET

This repository contains low-level bindings for MeshOptimizer used in Evergine.
This binding is generated from the MeshOptimizer release:
https://github.com/zeux/meshoptimizer/releases/tag/v0.23

[![CI](https://github.com/EvergineTeam/Meshoptimizer.NET/actions/workflows/CI.yml/badge.svg)](https://github.com/EvergineTeam/Meshoptimizer.NET/actions/workflows/CI.yml)
[![CD](https://github.com/EvergineTeam/Meshoptimizer.NET/actions/workflows/CD.yml/badge.svg)](https://github.com/EvergineTeam/Meshoptimizer.NET/actions/workflows/CD.yml)
[![Nuget](https://img.shields.io/nuget/v/Evergine.Bindings.MeshOptimizer?logo=nuget)](https://www.nuget.org/packages/Evergine.Bindings.MeshOptimizer)

## Purpose

When a GPU renders triangle meshes, various stages of the GPU pipeline have to process vertex and index data. The efficiency of these stages depends on the data you feed to them; this library provides algorithms to help optimize meshes for these stages, as well as algorithms to reduce the mesh complexity and storage overhead.

## Pipeline

When optimizing a mesh, you should typically feed it through a set of optimizations (the order is important!):

Indexing
(optional, discussed last) Simplification
Vertex cache optimization
Overdraw optimization
Vertex fetch optimization
Vertex quantization
Shadow indexing
(optional) Vertex/index buffer compression

Go to the original repository for more details:
https://github.com/zeux/meshoptimizer

## Supported Platforms

- [x] Windows x64, ARM64
- [x] Linux x64, ARM64
- [x] MacOS ARM64
