# Calculatrice .NET MAUI
 
Application de calculatrice développée avec **.NET MAUI** (C# et XAML), dont l'interface reprend le design des calculatrices iOS.
 
![Interface de la calculatrice](docs/screenshot.png)
 
## Fonctionnalités
 
- [x] Calculatrice basique : addition, soustraction, multiplication, division
- [x] Fonctions utilitaires : effacement (C), changement de signe (+/-), pourcentage (%)
- [x] Affichage de l'expression en cours et du résultat
- [ ] Mode scientifique (cosinus, exponentielle, etc.) : interface intégrée, **en cours de développement**
- [ ] Bascule degrés / radians (bouton `DEG` présent dans l'interface)
## Technologies
 
- .NET MAUI
- C# / XAML
- Architecture MVVM
## Architecture
 
Le projet suit le patron **MVVM** (Model – View – ViewModel) :
 
| Couche        | Rôle                                                                                   |
|---------------|----------------------------------------------------------------------------------------|
| **View**      | Pages XAML, boutons et affichage, sans logique métier                                  |
| **ViewModel** | État de l'affichage, commandes liées aux boutons, expression en cours de saisie        |
| **Model**     | Logique de calcul : module de base et module scientifique, indépendants l'un de l'autre |
 
> Adapter cette section avec la structure réelle des dossiers du projet.
 
## Prérequis
 
- [.NET SDK](https://dotnet.microsoft.com/download) (version utilisée par le projet : [à préciser])
- Workload .NET MAUI :
```bash
  dotnet workload install maui
```
 
- Visual Studio 2022 (ou VS Code avec l'extension .NET MAUI)
- Un émulateur Android ou un appareil physique
## Lancer le projet
 
```bash
git clone [URL-du-dépôt]
cd [nom-du-dossier]
dotnet build
```
 
Pour exécuter sur un émulateur Android :
 
```bash
dotnet build -t:Run -f net8.0-android
```
 
> Remplacer `net8.0-android` par la version cible définie dans le fichier `.csproj`.
 
## État d'avancement
 
La calculatrice basique est terminée et fonctionnelle. Les fonctions scientifiques sont intégrées à l'interface, mais ne sont pas encore opérationnelles : le branchement des boutons à la logique de calcul et les tests sont en cours.
 
## Auteur
 
**Agassem Sanda Florentin**
